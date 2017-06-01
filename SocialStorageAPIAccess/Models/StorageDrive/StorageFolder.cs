using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialStorageAPIAccess
{
    public class StorageFolder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StorageContentType StorageContentType { get; set; }
        public string DonwloadLink { get; set; }
        public string MimeType { get; set; }
        public string Parent { get; set; }
    }

   
}