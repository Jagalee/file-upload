using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace SocialStorageAPIAccess.StorageDrive
{
    public class DropboxStorage
    {
        string DropboxAccessToken = string.Empty;
        DropboxClient dropboxClient;
        public DropboxStorage(string dropboxAccessToken)
        {
            DropboxAccessToken = dropboxAccessToken;
            dropboxClient = new DropboxClient(DropboxAccessToken);
        }

        public List<StorageFolder> GetAll(string folderPath)
        {
            if (folderPath == "/")
            {
                folderPath = "";
            }
            var listFolder = new List<StorageFolder>();
            var folderList = dropboxClient.Files.ListFolderAsync(folderPath).Result;
            foreach (var item in folderList.Entries)
            {
                listFolder.Add(new StorageFolder
                {
                    Id = item.PathDisplay,
                    Name = item.Name,
                    MimeType = string.Empty,
                    DonwloadLink = string.Format("https://www.dropbox.com/home{0}", item.PathDisplay),
                    Parent = item.PathDisplay.Replace(Path.GetFileName(item.PathDisplay), ""),
                    StorageContentType = item.IsFolder ? StorageContentType.Folder : StorageContentType.File,
                });
            }
            return listFolder;

        }

        public bool Delete(string filePath)
        {
            try
            {
                var result = dropboxClient.Files.DeleteAsync(filePath).Result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Update(string oldFilePath, string newFilePath)
        {
            try
            {
                newFilePath = oldFilePath.Replace(Path.GetFileName(oldFilePath), newFilePath);
                var result = dropboxClient.Files.MoveAsync(oldFilePath, newFilePath).Result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CreateFile(byte[] byteArray, string filePath, string parent)
        {
            System.IO.MemoryStream mem = new System.IO.MemoryStream(byteArray);
            var updated = dropboxClient.Files.UploadAsync(parent + filePath,
                WriteMode.Overwrite.Instance,
                body: mem).Result;

        }
    }
}