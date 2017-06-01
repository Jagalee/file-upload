using System;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Configuration;
using System.Web.Mvc;

namespace SocialStorageAPIAccess
{
    public class GDriveAuthentication
    {
        private string[] scopes;
        private string applicationName;

        public GDriveAuthentication()
        {
            this.scopes = new String[] { DriveService.Scope.Drive };
            this.applicationName = "DotNETGoogleDriveAPI";
        }

        public GDriveAuthentication(string[] scopes)
        {
            this.scopes = scopes;
            this.applicationName = "DotNETGoogleDriveAPI";
        }

        public UserCredential GetUserCredential(string secretFilePath)
        {
            UserCredential userCredential;

            using (var stream = new FileStream(secretFilePath, FileMode.Open, FileAccess.Read))
            {
                string credentialPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credentialPath = Path.Combine(credentialPath, ".credentials/book-fetcher");

                userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credentialPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credentialPath);
            }

            return userCredential;
        }

        public DriveService GetDriveService()
        {
            UserCredential userCredential = GetUserCredential(System.Web.HttpContext.Current.Server.MapPath("~/client_secret.json"));

            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = applicationName,
            });

            return driveService;
        }
    }
}
