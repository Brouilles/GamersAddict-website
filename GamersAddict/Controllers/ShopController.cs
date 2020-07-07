using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GamersAddict.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return View();
        }

        // GET: PS4
        public ActionResult PS4()
        {
            return View();
        }

        // GET: XboxOne
        public ActionResult XboxOne()
        {
            return View();
        }
    }
}