using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialStorageAPIAccess.OneDrive
{
    public class OneDriveParentReference
    {
        public string driveId { get; set; }
        public string id { get; set; }
        public string path { get; set; }
    }
    public class OneDriveFolder
    {
        public int childCount { get; set; }
    }

    public class OneDriveValue
    {
        public string DownloadUrl { get; set; }
        public string createdDateTime { get; set; }
        public string cTag { get; set; }
        public string eTag { get; set; }
        public string id { get; set; }
        public string lastModifiedDateTime { get; set; }
        public string name { get; set; }
        public OneDriveParentReference parentReference { get; set; }
        public int size { get; set; }
        public string webUrl { get; set; }
        public OneDriveFolder folder { get; set; }
    }

    public class OneDriveRootObject
    {
        public List<OneDriveValue> value { get; set; }
    }
}