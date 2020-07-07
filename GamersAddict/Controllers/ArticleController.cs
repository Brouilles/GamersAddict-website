using GamersAddict.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GamersAddict.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index(int? page)
        {
            if (page == null)
                page = 0;

            ViewBag.Page = page;

            using (var context = new SiteDbContext())
            {
                if (page == 0)
                {
                    List<ArticlesViewModel> model = new List<ArticlesViewModel>();
                    model = context.Articles
                        .OrderByDescending(r => r.Date)
                        .Where(r => r.PublishState == 2)
                        .Skip(3)
                        .Take(6)
                        .Select(r => new ArticlesViewModel
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Description = r.Description,
                            Date = r.Date,
                            Views = r.Views,
                            PublishState = r.PublishState
                        }).ToList();

                    return View(model);
                }
                else
                {
                    List<ArticlesViewModel> model = new List<ArticlesViewModel>();
                    model = context.Articles
                        .OrderByDescending(r => r.Date)
                        .Where(r => r.PublishState == 2)
                        .Skip(6 * (int)page)
                        .Take(6)
                        .Select(r => new ArticlesViewModel
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Description = r.Description,
                            Date = r.Date,
                            Views = r.Views,
                            PublishState = r.PublishState
                        }).ToList();

                    return View(model);
                }
            }
        }

        // GET: Article
        public ActionResult Detail(string id)
        {
            using (var context = new SiteDbContext())
            {
                id = HttpUtility.UrlDecode(id);
                Articles model = context.Articles
                                     .Where(r => r.Title == id).FirstOrDefault();

                if (model == null)
                    return RedirectToAction("Index", "Article");

                if (model.PublishState == 0 && !(User.IsInRole("administrator") || User.IsInRole("writer")))
                    return RedirectToAction("Index", "Article");

                var applicationDbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                var author = applicationDbContext.Users
                           .Where(r => r.Id == model.AuthorId)
                           .Select(r => r.UserName).ToArray()[0];

                ViewBag.Author = author;
                ViewBag.Id = model.Id;
                ViewBag.Tags = model.Tags;

                return View(model);
            }
        }

        // POST: PostComment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PostComment(Comments model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var db = new SiteDbContext())
                {
                    bool ifAccountBan = true;
                    var userId = User.Identity.GetUserId();
                    ifAccountBan = db.AspNetUserBan
                        .Any(r => r.UserId.Equals(userId));

                    if (ifAccountBan)
                    {
                        Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //Add comment
                        model.UserId = User.Identity.GetUserId();
                        model.Date = DateTime.Now;

                        db.Comments.Add(model);
                        db.SaveChanges();
                    }
                }

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: DeleteComment
        [Authorize]
        public ActionResult DeleteComment(int id)
        {
            try
            {
                using (var context = new SiteDbContext())
                {
                    var model = context.Comments.Find(id);

                    if (model.UserId == User.Identity.GetUserId() || User.IsInRole("administrator") || User.IsInRole("writer") || User.IsInRole("moderator"))
                    {
                        context.Entry(model).State = System.Data.Entity.EntityState.Deleted;
                        context.SaveChanges();
                    }
                }

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Search
        public ActionResult Search(string page)
        {
            using (var context = new SiteDbContext())
            {
                page = HttpUtility.UrlDecode(page);
                ViewBag.Search = page;

                List<ArticlesViewModel> model = context.Articles
                    .OrderByDescending(r => r.Id)
                     .Where(r => r.Title.Contains(page) && r.PublishState == 2 
                     || r.Tags.Contains(page) && r.PublishState == 2)
                     .Select(r => new ArticlesViewModel
                     {
                         Id = r.Id,
                         Title = r.Title,
                         Description = r.Description,
                         Date = r.Date,
                         Views = r.Views,
                         PublishState = r.PublishState
                     }).Take(10).ToList();

                if (model == null)
                    return RedirectToAction("Index", "Article");

                return View(model);
            }
        }

        // GET: Tag
        public ActionResult Tag(string id)
        {
            using (var context = new SiteDbContext())
            {
                id = HttpUtility.UrlDecode(id);
                ViewBag.Tag = id;

                List<ArticlesViewModel> model = context.Articles
                    .OrderByDescending(r => r.Id)
                     .Where(r => r.Tags.Contains(id) && r.PublishState == 2)
                     .Select(r => new ArticlesViewModel
                     {
                         Id = r.Id,
                         Title = r.Title,
                         Description = r.Description,
                         Date = r.Date,
                         Views = r.Views,
                         PublishState = r.PublishState
                     }).Take(10).ToList();

                if (model == null)
                    return RedirectToAction("Index", "Article");

                return View(model);
            }
        }
    }
}