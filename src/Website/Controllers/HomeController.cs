using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            using (var db = new BusinessLogic.Data.SharingDataContext())
            {
                var count = db.StoredFiles.Where(x=>x.OwnerUsername == this.User.Identity.Name).Count();

                var yesterday = DateTime.UtcNow.AddDays(-1);

                ViewBag.Message = string.Format("You have {0} stored file(s).", count);

                ViewBag.OrphanSetCount = db.FileSets.Count(x => x.SharedFileSets.Count == 0);// && x.CreateDateTime < yesterday);

                ViewBag.OrphanFileCount = db.StoredFiles.Count(x => x.FileSet.SharedFileSets.Count == 0 && x.SharedFiles.Count == 0);// && x.CreateDateTime < yesterday);
                                
                ViewBag.ExpiredSetCount = db.SharedFileSets.Count(x => x.ExpirationDateTime < DateTime.UtcNow);

                ViewBag.ExpiredFileCount = db.SharedFiles.Count(x => x.ExpirationDateTime < DateTime.UtcNow);
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
