using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace SocialStorageAPIAccess
{
    /// <summary>
    /// This singleton class provide DriveService using UserCredential from class GDriveAuthentication
    /// </summary>
    public sealed class GDriveService
    {
        private static readonly GDriveService instance = new GDriveService();

        private GDriveAuthentication gDriveAuthentication;
        private DriveService driveService;

        static GDriveService()
        {

        }

        private GDriveService()
        {
            gDriveAuthentication = new GDriveAuthentication();
            driveService = gDriveAuthentication.GetDriveService();
        }

        public static GDriveService Instance
        {
            get
            {
                return instance;
            }
        }

        public DriveService GetDriveService()
        {
            return this.driveService;
        }
    }
}
