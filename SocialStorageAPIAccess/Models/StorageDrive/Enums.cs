using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialStorageAPIAccess
{
    public enum StorageContentType
    {
        Folder = 1,
        File = 2
    }

    public enum SocialStorageType
    {
        GoogleDrive = 1,
        OneDrive = 2,
        Dropbox = 3
    }
}