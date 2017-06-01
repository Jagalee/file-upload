using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SocialStorageAPIAccess.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            var folderAPi = new GDriveFolderAPI();
            //var folder = folderAPi.GetAll();
            //var xx = folderAPi.Get("0B-FlQeJGVwOxbWNlX3pjRVhUNm8");
            //var xx1 = folderAPi.GetAllFiels("0B-FlQeJGVwOxbWNlX3pjRVhUNm8");
            //folderAPi.Delete("168MCJc92n47V7uGYJ2CCG2Hl5N3Bwb4FZ4L052Z9UOo");
            //folderAPi.Update("1tCQyr1VZYl6cJn0Bm9VrbxaU7nLtNmOXnFt9Wyujdz8", "test-doc-new");
            //folderAPi.CreateFile(@"D:\KAZIN.txt", "0B-FlQeJGVwOxbWNlX3pjRVhUNm8");

            //List<string> folderName = new List<string>();
            //foreach (var item in folder.Result)
            //{
            //    folderName.Add(String.Format("{0} ({1})", item.Name, item.Id));
            //}

            return View();
        }

        

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}


