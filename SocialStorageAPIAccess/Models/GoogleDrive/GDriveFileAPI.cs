using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;

namespace SocialStorageAPIAccess
{
    public class GDriveFileAPI
    {
        private GDriveService gDriveService;
        private DriveService driveService;
        private string folderId;

        public string FolderId
        {
            get { return folderId; }
            set { folderId = value; }
        }

        public GDriveFileAPI()
        {
            this.gDriveService = GDriveService.Instance;
            this.driveService = this.gDriveService.GetDriveService();
        }

        /// <summary>
        /// Awaitable method, get file by fileId.
        /// </summary>
        /// <param name="fileId">Id of the file</param>
        /// <returns>A GoogleDriveFile</returns>
        public async Task<GDriveFile> GetAsync(string fileId)
        {
            if (String.IsNullOrEmpty(fileId))
            {
                try
                {
                    GDriveFile googleDriveFile = null;
                    FilesResource.GetRequest getRequest = driveService.Files.Get(fileId);
                    Google.Apis.Drive.v3.Data.File file = await getRequest.ExecuteAsync();

                    if (file.MimeType != "application/vnd.google-apps.drive-sdk")
                    {
                        throw new Exception("The founded object is not a file!");
                    }

                    googleDriveFile = Convert(file);
                    return googleDriveFile;
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// Get file by fileId.
        /// </summary>
        /// <param name="fileId">Id of the file</param>
        /// <returns>A GoogleDriveFile</returns>
        public GDriveFile Get(string fileId)
        {
            if (String.IsNullOrEmpty(fileId))
            {
                try
                {
                    GDriveFile googleDriveFile = null;
                    FilesResource.GetRequest getRequest = driveService.Files.Get(fileId);
                    Google.Apis.Drive.v3.Data.File file = getRequest.Execute();
                    googleDriveFile = Convert(file);
                    return googleDriveFile;
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// Awaitable method, get all files from drive.
        /// </summary>
        /// <returns>A list of GoogleDriveFile.</returns>
        public async Task<List<GDriveFile>> GetAllAsync()
        {
            try
            {
                FilesResource.ListRequest listRequest = driveService.Files.List();
                IList<Google.Apis.Drive.v3.Data.File> files = (await listRequest.ExecuteAsync()).Files.ToList();
                List<GDriveFile> listFile = new List<GDriveFile>();
                foreach (var file in files)
                {
                    if (file.MimeType == "application/vnd.google-apps.folder")
                    {
                        continue;
                    }
                    listFile.Add(Convert(file));
                }

                return listFile;
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Get all files from drive.
        /// </summary>
        /// <returns>A list of GoogleDriveFile.</returns>
        public List<GDriveFile> GetAll()
        {
            try
            {
                FilesResource.ListRequest listRequest = driveService.Files.List();
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files.ToList();
                List<GDriveFile> listFile = new List<GDriveFile>();
                foreach (var file in files)
                {
                    if (file.MimeType == "application/vnd.google-apps.folder")
                    {
                        continue;
                    }
                    listFile.Add(Convert(file));
                }

                return listFile;
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Awaitable method, get all files inside a folder by folderId.
        /// </summary>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>A list of GoogleDriveFile</returns>
        public async Task<List<GDriveFile>> GetFilesInFolderAsync(string folderId)
        {
            if (!String.IsNullOrEmpty(folderId))
            {
                try
                {

                    FilesResource.ListRequest listRequest = driveService.Files.List();
                    listRequest.Q = "'" + folderId + "' in parents";
                    listRequest.Fields = "files(*)";
                    IList<Google.Apis.Drive.v3.Data.File> files = (listRequest.ExecuteAsync().Result).Files;

                    List<GDriveFile> listFile = new List<GDriveFile>();
                    foreach (var file in files)
                    {
                        listFile.Add(Convert(file));
                    }
                    return listFile;
                }
                catch (Exception ex)
                {

                    throw;
                }
                
            }
            return null;
        }

        /// <summary>
        /// Get all files inside a folder by folderId.
        /// </summary>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>A list of GoogleDriveFile</returns>
        public List<GDriveFile> GetFilesInFolder(string folderId)
        {
            if (String.IsNullOrEmpty(folderId))
            {
                FilesResource.ListRequest listRequest = driveService.Files.List();
                listRequest.Q = "'" + folderId + "' in parents";
                listRequest.Fields = "files(*)";
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

                List<GDriveFile> listFile = new List<GDriveFile>();
                foreach (var file in files)
                {
                    listFile.Add(Convert(file));
                }

                return listFile;
            }
            return null;
        }

        /// <summary>
        /// Awaitable method, delete file by fileId.
        /// </summary>
        /// <param name="fileId">Id of the file.</param>
        /// <returns>true if deleted, false is failed to delete.</returns>
        public async Task<Boolean> DeleteAsync(string fileId)
        {
            if (!String.IsNullOrEmpty(fileId) && !String.IsNullOrWhiteSpace(fileId))
            {
                try
                {
                    FilesResource.DeleteRequest deleteRequest = driveService.Files.Delete(fileId);
                    string response = await deleteRequest.ExecuteAsync();
                    return String.IsNullOrEmpty(response) ? true : false;
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.Write(ex.Message);
                }
            }

            return false;
        }

        /// <summary>
        /// Delete file by fileId.
        /// </summary>
        /// <param name="fileId">Id of the file.</param>
        /// <returns>true if deleted, false is failed to delete.</returns>
        public bool Delete(string fileId)
        {
            if (!String.IsNullOrEmpty(fileId) && !String.IsNullOrWhiteSpace(fileId))
            {
                try
                {
                    FilesResource.DeleteRequest deleteRequest = driveService.Files.Delete(fileId);
                    string response = deleteRequest.Execute();
                    return String.IsNullOrEmpty(response) ? true : false;
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.Write(ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// Awaitable method, update the existed file on drive with new content.
        /// </summary>
        /// <param name="fileId">Id of the file.</param>
        /// <param name="contentPath">Path to the content file.</param>
        /// <param name="forceUpdate">If forceUpdate, the method will not check for the match of local file extension with drive file extension, else it will throw an exception.</param>
        /// <returns>Updated file.</returns>
        public async Task<GDriveFile> UpdateAsync(string fileId, string contentPath, bool forceUpdate = false)
        {
            try
            {
                FilesResource.GetRequest getRequest = driveService.Files.Get(fileId);
                Google.Apis.Drive.v3.Data.File driveFile = await getRequest.ExecuteAsync();
                if (!forceUpdate)
                {
                    //Local file extension
                    string fileName = System.IO.Path.GetFileName(contentPath);
                    string contentFileExtension = fileName.Split('.').Length > 1 ? fileName.Split('.')[1] : "";

                    //Server file extension
                    string driveFileExtension = driveFile.FileExtension;

                    //Throw an exception if two file extension is not the same 
                    if (contentFileExtension == driveFileExtension)
                    {
                        throw new Exception("File extension is not match!\nServer file: " + driveFileExtension + "\n Local file:" + contentFileExtension);
                    }
                }

                byte[] content = System.IO.File.ReadAllBytes(contentPath);
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(content);
                FilesResource.UpdateMediaUpload updateRequest =
                    driveService.Files.Update(driveFile, fileId, memoryStream, GetMimeType(contentPath));

                await updateRequest.UploadAsync();
                return Convert(updateRequest.ResponseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Update the existed file on drive with new content.
        /// </summary>
        /// <param name="fileId">Id of the file.</param>
        /// <param name="contentPath">Path to the content file.</param>
        /// <param name="forceUpdate">If forceUpdate, the method will not check for the match of local file extension with drive file extension, else it will throw an exception.</param>
        /// <returns>Updated file.</returns>
        public GDriveFile Update(string fileId, string contentPath, bool forceUpdate = false)
        {
            try
            {
                FilesResource.GetRequest getRequest = driveService.Files.Get(fileId);
                Google.Apis.Drive.v3.Data.File driveFile = getRequest.Execute();
                if (!forceUpdate)
                {
                    //Local file extension
                    string fileName = System.IO.Path.GetFileName(contentPath);
                    string contentFileExtension = fileName.Split('.').Length > 1 ? fileName.Split('.')[1] : "";

                    //Server file extension
                    string driveFileExtension = driveFile.FileExtension;

                    //Throw an exception if two file extension is not the same 
                    if (contentFileExtension == driveFileExtension)
                    {
                        throw new Exception("File extension is not match!\nServer file: " + driveFileExtension + "\n Local file:" + contentFileExtension);
                    }
                }

                byte[] content = System.IO.File.ReadAllBytes(contentPath);
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(content);
                FilesResource.UpdateMediaUpload updateRequest =
                    driveService.Files.Update(driveFile, fileId, memoryStream, GetMimeType(contentPath));

                updateRequest.Upload();
                return Convert(updateRequest.ResponseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Convert Google.Apis.Drive.v3.Data.File to new object type GoogleDriveFile
        /// </summary>
        /// <param name="file">File type Google.Apis.Drive.v3.Data.File</param>
        /// <returns>Object type GoogleDriveFolder</returns>
        private GDriveFile Convert(Google.Apis.Drive.v3.Data.File file)
        {
            if (file == null) return null;

            return new GDriveFile
            {
                Id = file.Id,
                Name = file.Name,
                FileExtension = file.FileExtension,
                Size = file.Size,
                ThumbnailUrl = file.ThumbnailLink,
                WebContentLink = file.WebContentLink,
                WebViewLink = file.WebViewLink,
                MimeType = file.MimeType,
                Description = file.Description,
                CreatedTime = file.CreatedTime,
                ModifiedTime = file.ModifiedTime,
                Parents = file.Parents
            };
        }

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }

    public class GDriveFile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public long? Size { get; set; }

        public string FileExtension { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public string Description { get; set; }

        public string MimeType { get; set; }

        public IList<String> Parents { get; set; }

        public string WebContentLink { get; set; }

        public string WebViewLink { get; set; }

        public string ThumbnailUrl { get; set; }

        public GDriveFile()
        {
            this.MimeType = "application/vnd.google-apps.drive-sdk";
        }
    }
}
