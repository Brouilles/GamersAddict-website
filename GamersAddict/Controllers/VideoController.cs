using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GamersAddict.Controllers
{
    public class VideoController : Controller
    {
        // GET: Video
        public ActionResult Player(string id)
        {
            if (id == null)
                return RedirectToAction("Video", "Home");

            ViewBag.id = id;
            return View();
        }
    }
}