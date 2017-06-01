using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SocialStorageAPIAccess.OneDrive
{
    public class OneDriveStorageApi : O365RestSessionBase
    {
        /// <summary>
        /// The oneDrive REST API root address
        /// </summary>
        public string OneDriveApiRoot { get; set; } = "https://api.onedrive.com/v1.0/";

        /// <summary>
        /// Init a O365RestSessionBase object
        /// </summary>
        /// <param name="clientId">clientId of you office 365 application, you can find it in https://apps.dev.microsoft.com/</param>
        /// <param name="clientSecret">Password/Public Key of you office 365 application, you can find it in https://apps.dev.microsoft.com/</param>
        /// <param name="redirectURI">Authentication callback url, you can set it in https://apps.dev.microsoft.com/</param>
        public OneDriveStorageApi(string clientId, string clientSecret, string redirectURI) : base(clientId, clientSecret, redirectURI)
        {
        }

        /// <summary>
        /// Upload file to onedrive
        /// </summary>
        /// <param name="filePath">file path in local dick</param>
        /// <param name="oneDrivePath">path of one dirve</param>
        /// <returns>uploaded file info with json format</returns>
        public void CreateFile(byte[] byteArray, string fileName, string parent)
        {
            #region create upload session

            var folderDetail = GetFolderDetail(parent);
            string uploadUri = GetUploadSession(folderDetail.Parent + "/" + fileName);
            #endregion

            long position = 0;
            long totalLength = byteArray.Length;
            while (true)
            {
                if (position >= totalLength)
                {
                    break;
                }
                var result = UploadFileFragmentAsync(byteArray, uploadUri, position, totalLength).Result;
                position += byteArray.Length;
            }
        }

        /// <summary>
        /// Get ond drive share link by fileID
        /// </summary>
        /// <param name="fileID">id on office 365 of the file</param>
        /// <param name="type">the link type, about more:https://dev.onedrive.com/items/sharing_createLink.htm#link-types</param>
        /// <param name="scope">the scope of share, about more:https://dev.onedrive.com/items/sharing_createLink.htm#scope-types</param>
        /// <returns>Share Uri</returns>
        public async Task<string> GetShareLinkAsync(string fileID, OneDriveShareLinkType type, OneDrevShareScopeType scope)
        {
            string param = "{type:'" + type + "',scope:'" + scope + "'}";

            string result = await AuthRequestToStringAsync(
                uri: $"{OneDriveApiRoot}drive/items/{fileID}/action.createLink",
                httpMethod: HTTPMethod.Post,
                data: Encoding.UTF8.GetBytes(param),
                contentType: "application/json");

            return JObject.Parse(result).SelectToken("link.webUrl").Value<string>();
        }

        public List<StorageFolder> GetAll(string folderId)
        {
            var listFolder = new List<StorageFolder>();
            string url;
            if (string.IsNullOrWhiteSpace(folderId))
            {
                url = $"{OneDriveApiRoot}drive/root/children";
            }
            else
            {
                url = $"{OneDriveApiRoot}drive/items/" + folderId + "/children";
            }
            string result = AuthRequestToStringAsync(
                uri: url,
                httpMethod: HTTPMethod.Get,
                contentType: "application/json").Result;

            string folderList = JObject.Parse(result).Last.ToString();
            folderList = folderList.Replace("@content.downloadUrl", "DownloadUrl");
            var oneDriveRootObjectList = JsonConvert.DeserializeObject<OneDriveRootObject>("{" + folderList + "}");
            foreach (var folder in oneDriveRootObjectList.value)
            {
                listFolder.Add(new StorageFolder
                {
                    Id = folder.id,
                    Name = folder.name,
                    MimeType = "",
                    DonwloadLink = folder.DownloadUrl,
                    StorageContentType = folder.folder != null ? StorageContentType.Folder : StorageContentType.File,
                    Parent = folder.parentReference.id
                });
            }
            return listFolder;
        }

        public StorageFolder GetFolderDetail(string folderId)
        {
            var listFolder = new List<StorageFolder>();
            string url = $"{OneDriveApiRoot}drive/items/" + folderId + "/children";
            string result = AuthRequestToStringAsync(
                uri: url,
                httpMethod: HTTPMethod.Get,
                contentType: "application/json").Result;

            string folderList = JObject.Parse(result).Last.ToString();
            folderList = folderList.Replace("@content.downloadUrl", "DownloadUrl");
            var oneDriveRootObjectList = JsonConvert.DeserializeObject<OneDriveRootObject>("{" + folderList + "}");
            foreach (var folder in oneDriveRootObjectList.value)
            {
                listFolder.Add(new StorageFolder
                {
                    Id = folder.id,
                    Name = folder.name,
                    MimeType = "",
                    DonwloadLink = folder.DownloadUrl,
                    StorageContentType = folder.folder != null ? StorageContentType.Folder : StorageContentType.File,
                    Parent = folder.parentReference.path
                });
            }
            return listFolder[0];
        }

        public bool Delete(string itemId)
        {
            try
            {
                string url = $"{OneDriveApiRoot}drive/items/" + itemId;
                string result = AuthRequestToStringAsync(
                    uri: url,
                    httpMethod: HTTPMethod.Delete,
                    contentType: "application/json").Result;
                return true;
            }
            catch (System.Exception)
            {
                return false;
                throw;
            }
        }

        public bool Update(string itemId, string newName)
        {
            try
            {
                string param = "{name:'" + newName + "'}";
                string url = $"{OneDriveApiRoot}drive/items/" + itemId;
                string result = AuthRequestToStringAsync(
                    uri: url,
                    data: Encoding.UTF8.GetBytes(param),
                    httpMethod: HTTPMethod.Patch,
                    contentType: "application/json").Result;
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// create a upload session for upload
        /// </summary>
        /// <param name="oneDriveFilePath">path of one dirve</param>
        /// <returns>upload Uri</returns>
        private string GetUploadSession(string oneDriveFilePath)
        {
            string url = ($"{OneDriveApiRoot}{oneDriveFilePath.Remove(0, 1)}:/upload.createSession");
            var uploadSession = AuthRequestToStringAsync(
                uri: url,
                httpMethod: HTTPMethod.Post,
                contentType: "application/x-www-form-urlencoded").Result;

            JObject jo = JObject.Parse(uploadSession);

            return jo.SelectToken("uploadUrl").Value<string>();
        }


        /// <summary>
        /// upload file fragment
        /// </summary>
        /// <param name="datas">file fragment</param>
        /// <param name="uploadUri">upload uri</param>
        /// <param name="position">postion of the file bytes</param>
        /// <param name="totalLength">the file bytes lenght</param>
        /// <returns>expire time with json format</returns>
        private async Task<string> UploadFileFragmentAsync(byte[] datas, string uploadUri, long position, long totalLength)
        {
            var request = await InitAuthRequest(uploadUri, HTTPMethod.Put, datas, null);
            request.Request.Headers.Add("Content-Range", $"bytes {position}-{position + datas.Length - 1}/{totalLength}");

            return await request.GetResponseStringAsync();
        }

        /// <summary>
        /// read file fragment
        /// </summary>
        /// <param name="stream">file stream</param>
        /// <param name="startPos">start position</param>
        /// <param name="count">take count</param>
        /// <returns>the fragment of file with byte[]</returns>
        private async Task<byte[]> ReadFileFragmentAsync(FileStream stream, long startPos, int count)
        {
            if (startPos >= stream.Length || startPos < 0 || count <= 0)
                return null;

            long trimCount = startPos + count > stream.Length ? stream.Length - startPos : count;

            byte[] retBytes = new byte[trimCount];
            stream.Seek(startPos, SeekOrigin.Begin);
            await stream.ReadAsync(retBytes, 0, (int)trimCount);
            return retBytes;
        }
    }
}
