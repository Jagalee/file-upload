using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//https://gdriveapi.codeplex.com/SourceControl/latest#GDriveFolderAPI.cs
namespace SocialStorageAPIAcces
{
    public class GoogleDrive
    {
        //var folderAPi = new GDriveFolderAPI();
        //var folder = folderAPi.GetAll();
    }
}





//var auth = new GDriveAuthentication();
//var authResult = auth.GetUserCredential(Server.MapPath("~/client_secret.json"));




//UserCredential credential;

//            using (var stream =
//                new FileStream(Server.MapPath("~/client_secret.json"), FileMode.Open, FileAccess.Read))
//            {
//                string credPath = System.Environment.GetFolderPath(
//                    System.Environment.SpecialFolder.Personal);
//credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");
//                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
//                    GoogleClientSecrets.Load(stream).Secrets,
//                    Scopes,
//                    "user",
//                    CancellationToken.None,
//                    new FileDataStore(credPath, true)).Result;
//                Console.WriteLine("Credential file saved to: " + credPath);
//            }

//            // Create Drive API service.
//            var service = new DriveService(new BaseClientService.Initializer()
//            {
//                HttpClientInitializer = credential,
//                ApplicationName = ApplicationName,

//            });

////#region MyRegion

////// Define parameters of request.
////FilesResource.ListRequest listRequest = service.Files.List();
////listRequest.PageSize = 10;
////listRequest.Fields = "nextPageToken, files(id, name)";

////// List files.
////IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
////    .Files;
////Console.WriteLine("Files:");
////if (files != null && files.Count > 0)
////{
////    foreach (var file in files)
////    {
////        Console.WriteLine("{0} ({1})", file.Name, file.Id);
////    }
////}
////else
////{
////    Console.WriteLine("No files found.");
////}

////#endregion

//List<string> folderName = new List<string>();
//folderName.Add("asd");
//            var request = service.Files.List();
//request.PageSize = 1000;
//            request.Q = "mimeType='application/vnd.google-apps.folder'";
//            request.Spaces = "drive";
//            request.Fields = "files(id, name,parents)";

//            var result = request.Execute();

//            //var request1 = service.Files.List();
//            //request1.Spaces = "appDataFolder";
//            //request1.Fields = "files(id, name)";
//            //request1.PageSize = 10;
//            //var result1 = request1.Execute();

//            foreach (var file in result.Files)
//            {
//                if (file.Parents != null)
//                {
//                    folderName.Add(String.Format(
//                        "Found file: {0} ({1})     |||   {2}", file.Name, file.Id, file.Parents[0]));
//                }
//                else
//                {
//                    folderName.Add(String.Format(
//                       "Found file: {0} ({1})", file.Name, file.Id));
//                }

//            }