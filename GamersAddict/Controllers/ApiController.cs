using GamersAddict.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GamersAddict.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api/Live
        public ActionResult Live()
        {
            Conf modelConf;
            using (var context = new SiteDbContext())
            {
                modelConf = context.Conf.Find(1);
            }

            if(modelConf.Value == null || modelConf.Value == string.Empty)
                return Json(new { InLive = false }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { InLive = true, Title = modelConf.Name, Id = modelConf.Value }, JsonRequestBehavior.AllowGet);
        }
    }
}