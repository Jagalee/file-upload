using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace SocialStorageAPIAccess
{
    public class GDriveFolderAPI
    {
        private GDriveService gDriveService;
        private DriveService driveService;

        public GDriveFolderAPI()
        {
            this.gDriveService = GDriveService.Instance;
            this.driveService = this.gDriveService.GetDriveService();
        }

        /// <summary>
        /// Get all folders include child folders and parent folders.
        /// </summary>
        /// <returns>A list of GoogleDriveFolder</returns>
        public List<StorageFolder> GetAll(string myDriveFolderId)
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            //listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
            listRequest.Fields = "files(id,name,parents,mimeType,webContentLink,webViewLink)";
            listRequest.PageSize = 1000;
            IList<Google.Apis.Drive.v3.Data.File> folders = ((listRequest.Execute()).Files.ToList());

            var listFolder = new List<StorageFolder>();
            GDriveFileAPI fileApi = new GDriveFileAPI();
            foreach (var folder in folders)
            {
                if (folder.Parents != null && (folder.Parents[0] == myDriveFolderId || myDriveFolderId == ""))
                {
                    var isRootFolder = false;
                    if (string.IsNullOrWhiteSpace(myDriveFolderId))
                    {
                        var folderInfo = Get(folder.Parents[0]);
                        if (string.Compare(folderInfo.Name, "My Drive", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            isRootFolder = true;
                            myDriveFolderId = folderInfo.Id;
                        }
                    }
                    else { isRootFolder = true; }

                    if (isRootFolder)
                    {
                        listFolder.Add(new StorageFolder
                        {
                            Id = folder.Id,
                            Name = folder.Name,
                            MimeType = folder.MimeType,
                            DonwloadLink = folder.WebViewLink,
                            StorageContentType = folder.MimeType == "application/vnd.google-apps.folder" ? StorageContentType.Folder : StorageContentType.File,
                            Parent = myDriveFolderId
                        });
                    }
                }
            }
            return listFolder;
        }


        public async Task<List<GDriveFolder>> GetAllFiels(string folderId)
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            //listRequest.Q = "mimeType = 'application/x-rar-compressed'";
            listRequest.Fields = "files(*)";
            listRequest.PageSize = 1000;
            IList<Google.Apis.Drive.v3.Data.File> folders = ((listRequest.ExecuteAsync().Result).Files.ToList());

            List<GDriveFolder> listFolder = new List<GDriveFolder>();
            GDriveFileAPI fileApi = new GDriveFileAPI();
            foreach (var folder in folders)
            {
                if (folder.Name.Contains("test-doc"))
                {
                    string s = "";
                }
                if (folder.Parents != null && (folder.Parents[0] == folderId))
                {
                    listFolder.Add(new GDriveFolder
                    {
                        Id = folder.Id,
                        Name = folder.Name,
                        MimeType = folder.MimeType,
                        WebViewLink = folder.WebViewLink,
                        WebContentLink = "",
                        Parents = folder.Parents,
                        Contents = await fileApi.GetFilesInFolderAsync(folder.Id),
                        CreatedTime = folder.CreatedTime,
                        ModifiedTime = folder.ModifiedTime
                    });
                }
            }


            return listFolder;
        }

        /// <summary>
        /// Get folder by folderId
        /// </summary>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>A GoogleDriveFolder if the folder is found or null.</returns>
        public GDriveFolder Get(string folderId)
        {
            if (!String.IsNullOrEmpty(folderId) && !String.IsNullOrWhiteSpace(folderId))
            {
                Google.Apis.Drive.v3.Data.File folder = null;
                try
                {
                    FilesResource.GetRequest getRequest = driveService.Files.Get(folderId);
                    folder = getRequest.ExecuteAsync().Result;

                    if (folder == null) return null;

                    GDriveFileAPI fileApi = new GDriveFileAPI();
                    var result = new GDriveFolder
                    {
                        Id = folder.Id,
                        Name = folder.Name,
                        MimeType = folder.MimeType,
                        WebViewLink = folder.WebViewLink,
                        WebContentLink = "",
                        Parents = folder.Parents,
                        Contents = fileApi.GetFilesInFolderAsync(folder.Id).Result,
                        CreatedTime = folder.CreatedTime,
                        ModifiedTime = folder.ModifiedTime
                    };
                    return result;
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.Write(ex.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Create a folder
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="parentId">Parent folder id.</param>
        /// <returns>A GoogleDriveFolder if it's created successfully, null if not.</returns>
        public async Task<GDriveFolder> Create(string folderName, string parentId = null)
        {
            if (String.IsNullOrEmpty(folderName) || String.IsNullOrWhiteSpace(folderName))
            {
                try
                {
                    // Create parents list
                    List<String> parents = null;
                    if (parents != null)
                    {
                        parents.Add(parentId);
                    }

                    // Make a create request
                    Google.Apis.Drive.v3.Data.File folder = null;
                    FilesResource.CreateRequest createRequest = driveService.Files.Create(
                        new Google.Apis.Drive.v3.Data.File
                        {
                            MimeType = "application/vnd.google-apps.folder",
                            Name = folderName,
                            Parents = parents
                        });
                    folder = await createRequest.ExecuteAsync();

                    return Convert(folder);
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.Write(ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// Permanent delete file from server, not moving to trash.
        /// </summary>
        /// <param name="folderId">Id of the folder.</param>
        public bool Delete(string folderId)
        {
            try
            {
                FilesResource.DeleteRequest deleteRequest = driveService.Files.Delete(folderId);
                deleteRequest.Execute();
                return true;
            }
            catch (Google.GoogleApiException)
            {
                return false;
            }
        }

        /// <summary>
        /// Update existed folder with new metadata.
        /// </summary>
        /// <param name="folderId">Id of the folder.</param>
        /// <param name="folderMetadata">Folder metadata</param>
        /// <returns></returns>
        public bool Update(string folderId, string newName)
        {
            try
            {
                FilesResource.UpdateRequest updateRequest = driveService.Files.Update(new Google.Apis.Drive.v3.Data.File
                {
                    Name = newName
                }, folderId);

                Google.Apis.Drive.v3.Data.File folder = updateRequest.Execute();
                return true;
            }
            catch (Google.GoogleApiException)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert Google.Apis.Drive.v2.Data.File to new object type GoogleDriveFolder.
        /// </summary>
        /// <param name="folder">Folder type Google.Apis.Drive.v3.Data.File</param>
        /// <returns>Object type GoogleDriveFile</returns>
        private GDriveFolder Convert(Google.Apis.Drive.v3.Data.File folder)
        {
            return new GDriveFolder
            {
                Id = folder.Id,
                Name = folder.Name,
                ThumbnailUrl = folder.ThumbnailLink,
                WebViewLink = folder.WebViewLink,
                Description = folder.Description,
                CreatedTime = folder.CreatedTime,
                ModifiedTime = folder.ModifiedTime,
                Parents = folder.Parents
            };
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public void CreateFile(byte[] byteArray, string fileName, string folderId)
        {
            List<String> parents = new List<string>();
            parents.Add(folderId);
            File body = new File();
            body.Name = fileName;
            body.Description = "File uploaded by Diamto Drive Sample";
            body.MimeType = GetMimeType(fileName);
            body.Parents = parents;

            // File's content.
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                var request = driveService.Files.Create(body, stream, GetMimeType(fileName));
                //request.Convert = true;   // uncomment this line if you want files to be converted to Drive format
                request.Upload();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                // return null;
            }
        }
    }

    public class GDriveFolder : GDriveFile
    {
        public IList<GDriveFile> Contents { get; set; }

        public GDriveFolder()
        {
            this.MimeType = "application/vnd.google-apps.folder";
        }
    }
}
