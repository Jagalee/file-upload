using Dropbox.Api;
using SocialStorageAPIAccess.OneDrive;
using SocialStorageAPIAccess.StorageDrive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SocialStorageAPIAccess.Controllers
{

    public class StorageDriveController : Controller
    {
        #region [Property]

        private string DropboxAppKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DropboxAppKey"].ToString();
            }
        }
        private string DropboxAppSercet
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DropboxAppSecret"].ToString();
            }
        }
        private string OneDriveClientId
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["OneDriveClientId"].ToString();
            }
        }
        private string OneDriveSecretKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["OneDriveSecretKey"].ToString();
            }
        }
        private string OneDriveCallbackURL
        {
            get
            {
                if (this.Request.Url.Host.ToLowerInvariant() == "localhost")
                {
                    return string.Format("http://{0}:{1}/StorageDrive/OnOneDriveAuthComplate", this.Request.Url.Host, this.Request.Url.Port);
                }

                var builder = new UriBuilder(
                    Uri.UriSchemeHttps,
                    this.Request.Url.Host);

                builder.Path = "/StorageDrive/OnOneDriveAuthComplate";

                return builder.ToString();
            }
        }
        private string DropboxCallbackURL
        {
            get
            {
                if (this.Request.Url.Host.ToLowerInvariant() == "localhost")
                {
                    return string.Format("http://{0}:{1}/StorageDrive/DropboxAuth", this.Request.Url.Host, this.Request.Url.Port);
                }

                var builder = new UriBuilder(
                    Uri.UriSchemeHttps,
                    this.Request.Url.Host);

                builder.Path = "/StorageDrive/DropboxAuth";

                return builder.ToString();
            }
        }

        #endregion

        #region [Authentication]

        public OneDriveStorageApi OneDriveSession
        {
            get
            {
                var oneDriveAccessToken = Session["OneDriveAccessToken"];
                if (oneDriveAccessToken == null)
                {
                    oneDriveAccessToken = new OneDriveStorageApi(OneDriveClientId, OneDriveSecretKey, OneDriveCallbackURL);
                    Session["OneDriveAccessToken"] = oneDriveAccessToken;
                }
                return oneDriveAccessToken as OneDriveStorageApi;
            }
        }

        public async Task<RedirectResult> OnOneDriveAuthComplate(string code)
        {
            await OneDriveSession.RedeemTokensAsync(code);
            return new RedirectResult("Index?type=" + (int)SocialStorageType.OneDrive);
        }

        public string DropboxConnectWithApi()
        {
            var redirect = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Code,
                DropboxAppKey,
                DropboxCallbackURL);

            return redirect.ToString();
        }

        public ActionResult DropboxAuth(string code, string state)
        {
            var response = DropboxOAuth2Helper.ProcessCodeFlowAsync(
                     code,
                     DropboxAppKey,
                     DropboxAppSercet,
                     DropboxCallbackURL).Result;
            Session["DropboxAccessToken"] = response.AccessToken;
            return RedirectToAction("Index", new { type = 3 });
        }

        #endregion

        #region [Action Method]

        [HttpGet]
        public ActionResult Index(int type = 0)
        {
            if (type == 0 || type == 1)
            {
                type = (int)SocialStorageType.GoogleDrive;
                ViewBag.FolderName = "";
            }
            else if (type == 2)
            {
                ViewBag.FolderName = "";
                if (string.IsNullOrEmpty(OneDriveSession.AccessCode))
                {
                    string url = OneDriveSession.GetLoginUrl("onedrive.appfolder");
                    return new RedirectResult(url);
                }
            }
            else if (type == 3)
            {
                var dropboxAccessToken = Session["DropboxAccessToken"];
                if (dropboxAccessToken == null)
                {
                    var url = DropboxConnectWithApi();
                    return new RedirectResult(url);
                }
                ViewBag.FolderName = "/";
            }
            ViewBag.SelectedType = type;
            return View();
        }

        [HttpPost]
        public PartialViewResult ListAll(int type, string folderId = "")
        {
            var result = new List<StorageFolder>();
            if (type == (int)SocialStorageType.GoogleDrive)
            {
                var folderAPi = new GDriveFolderAPI();
                result = folderAPi.GetAll(folderId);
            }
            else if (type == (int)SocialStorageType.OneDrive)
            {
                result = OneDriveSession.GetAll(folderId);
            }
            else if (type == (int)SocialStorageType.Dropbox)
            {
                var dropboxStorage = new DropboxStorage(Convert.ToString(Session["DropboxAccessToken"]));
                result = dropboxStorage.GetAll(folderId);
            }
            return PartialView(result);
        }

        [HttpPost]
        public JsonResult DeleteFile(string fileId, int type)
        {
            bool result = false;
            if (type == (int)SocialStorageType.GoogleDrive)
            {
                var folderAPi = new GDriveFolderAPI();
                result = folderAPi.Delete(fileId);
            }
            else if (type == (int)SocialStorageType.OneDrive)
            {
                result = OneDriveSession.Delete(fileId);
            }
            else if (type == (int)SocialStorageType.Dropbox)
            {
                var dropboxStorage = new DropboxStorage(Convert.ToString(Session["DropboxAccessToken"]));
                result = dropboxStorage.Delete(fileId);
            }
            return Json(new { Success = result });
        }

        [HttpPost]
        public JsonResult RenameFileName(string fileId, string newName, int type)
        {
            bool result = false;
            if (type == (int)SocialStorageType.GoogleDrive)
            {
                var folderAPi = new GDriveFolderAPI();
                result = folderAPi.Update(fileId, newName);
            }
            else if (type == (int)SocialStorageType.OneDrive)
            {
                result = OneDriveSession.Update(fileId, newName);
            }
            else if (type == (int)SocialStorageType.Dropbox)
            {
                var dropboxStorage = new DropboxStorage(Convert.ToString(Session["DropboxAccessToken"]));
                result = dropboxStorage.Update(fileId, newName); // fileId =  old file structure  and newName = New file
            }
            return Json(new { Success = result });
        }

        [HttpPost]
        public ActionResult UploadFiles()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fileName;
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fileName = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fileName = file.FileName;
                        }

                        MemoryStream target = new MemoryStream();
                        file.InputStream.CopyTo(target);
                        byte[] data = target.ToArray();

                        string parent = Request.Params["Parent"];
                        int type = Convert.ToInt32(Request.Params["Type"]);

                        if (type == (int)SocialStorageType.GoogleDrive)
                        {
                            var folderAPi = new GDriveFolderAPI();
                            folderAPi.CreateFile(data, fileName, parent);
                        }
                        else if (type == (int)SocialStorageType.OneDrive)
                        {
                            OneDriveSession.CreateFile(data, fileName, parent);
                        }
                        else if (type == (int)SocialStorageType.Dropbox)
                        {
                            var dropboxStorage = new DropboxStorage(Convert.ToString(Session["DropboxAccessToken"]));
                            dropboxStorage.CreateFile(data, fileName, parent);  // filename = filename with full path
                        }

                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        #endregion
    }
}