using EasySiteMap;
using EasySiteMap.Extensions;
using GamersAddict.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Postal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace GamersAddict.Controllers
{
    [Authorize(Roles = "administrator, writer")]
    public class AdminController : Controller
    {
        /*************************************/
        /*               INDEX               */
        /*************************************/
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // POST : InLive
        [Authorize(Roles = "administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InLive(Conf model)
        {
            var modelConf = new Conf();
            using (var context = new SiteDbContext())
            {
                modelConf = context.Conf.Find(1);
            }

            using (var context = new SiteDbContext())
            {
                if (modelConf.Value == "")
                {
                    context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    var checkBox = Request.Form["preventEmail"];
                    if (checkBox == "true")
                    {
                        using (var db = new SiteDbContext())
                        {
                            var HeaderModel = context.Articles
                                    .OrderByDescending(r => r.Date)
                                    .Where(r => r.PublishState == 2)
                                    .Take(2)
                                    .Select(r => new ArticlesViewModel
                                    {
                                        Id = r.Id,
                                        Title = r.Title,
                                        Description = r.Description,
                                        Date = r.Date,
                                        Views = r.Views,
                                        PublishState = r.PublishState
                                    }).ToArray();

                            List<YouTubeAlertRegistration> newsletterModel = db.YouTubeAlertRegistration.ToList();
                            newsletterModel.ForEach(delegate (YouTubeAlertRegistration user) //Send for each user
                            {
                                dynamic email = new Email("YouTubeAlertEmail");
                                email.To = user.Email;
                                email.Title = "[Gamers Addict] " + model.Name;
                                email.Text = "J\'ai commencé une diffusion en direct du nom de " + model.Name + " sur YouTube: <a href=\"https://www.youtube.com/watch?v=" + model.Value + "\">https://www.youtube.com/watch?v=" + model.Value + "</a>";
                                email.Email = user.Email;
                                email.SecretKey = user.SecretKey;

                                email.ArticleOneId = HeaderModel[0].Id;
                                email.ArticleOneTitle = HeaderModel[0].Title;
                                email.ArticleOneDescription = HeaderModel[0].Description;

                                email.ArticleTwoId = HeaderModel[1].Id;
                                email.ArticleTwoTitle = HeaderModel[1].Title;
                                email.ArticleTwoDescription = HeaderModel[1].Description;
                                email.Send();
                            });
                        }
                    }

                    return Json("Début du live !", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.Name = "";
                    model.Value = "";
                    context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    return Json("Fin du live !", JsonRequestBehavior.AllowGet);
                }
            }
        }

        /*************************************/
        /*                SEO                */
        /*************************************/
        // GET: Admin/SEO
        public ActionResult SEO()
        {
            return View();
        }

        // POST: Admin/UpdateConf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateConf(Conf model)
        {
            using (var context = new SiteDbContext())
            {
                context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
            }

            Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/UpdateConf
        public ActionResult UpdateSitemapXML()
        {
            UrlHelper urlHelper = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);

            List<SiteMapItem> list = new List<SiteMapItem>();
            list.Add(new SiteMapItem(urlHelper.QualifiedAction("Index", "Home")));
            list.Add(new SiteMapItem(urlHelper.QualifiedAction("Video", "Home")));
            list.Add(new SiteMapItem(urlHelper.QualifiedAction("Index", "Article")));

            List<ArticlesViewModel> model = new List<ArticlesViewModel>();
            using (var context = new SiteDbContext())
            {
                model = context.Articles
                    .Where(r => r.PublishState == 2)
                    .OrderByDescending(r => r.Id)
                    .Select(r => new ArticlesViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Date = r.Date,
                        Views = r.Views,
                        PublishState = r.PublishState
                    }).Take(25).ToList();
            }

            foreach (var article in model)
            {
                SiteMapItem itemmap = new SiteMapItem(Request.Url.Host + "/Article/Detail/" + HttpUtility.UrlEncode(article.Title), article.Date, ChangeFrequency.Weekly, 0.5);
                list.Add(itemmap);
            }

            EasySiteMapGenerator g = new EasySiteMapGenerator(new SiteMapConfig()
            {
                Domain = Request.Url.Host + "/",
                LocalFile = Server.MapPath("~"),
                TotalItensByFile = 50000
            });
            g.GenerateSiteMap(list);

            Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        /*************************************/
        /*             REDACTION             */
        /*************************************/
        /*** TO DO LIST ***/
        // GET: Admin/Redaction
        [Authorize(Roles = "administrator, writer")]
        public ActionResult Redaction()
        {
            return View();
        }

        // POST: Admin/Redaction
        [Authorize(Roles = "administrator, writer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Redaction(ItemsToDoList model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Erreur dans le formulaire");
                return View(model);
            }

            using (var context = new SiteDbContext())
            {
                model.State = 0;
                context.ItemsToDoList.Add(model);
                context.SaveChanges();
            }

            return RedirectToAction("Redaction", "Admin");
        }

        // POST: Admin/RedactionToDoListDelete
        [Authorize(Roles = "administrator, writer")]
        public ActionResult RedactionToDoListDelete(int id)
        {
            using (var context = new SiteDbContext())
            {
                ItemsToDoList team = context.ItemsToDoList.Find(id);
                context.Entry(team).State = System.Data.Entity.EntityState.Deleted;
                context.SaveChanges();
            }

            return RedirectToAction("Redaction", "Admin");
        }

        // POST: Admin/RedactionToDoListAjaxUpdate
        [Authorize(Roles = "administrator, writer")]
        public ActionResult RedactionToDoListAjaxUpdate(int id, int state)
        {
            using (var context = new SiteDbContext())
            {
                ItemsToDoList item = context.ItemsToDoList.Find(id);
                item.State = state;

                context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        /*** ARTICLE ***/
        // GET: Admin/Articles
        [Authorize(Roles = "administrator, writer")]
        public ActionResult Articles(string searchTerm = null)
        {
            using (var context = new SiteDbContext())
            {
                if (searchTerm != null)
                    ViewBag.searchTerm = searchTerm;

                List<ArticlesViewModel> model = new List<ArticlesViewModel>();
                model = context.Articles
                    .Where(r => searchTerm == null || r.Title.StartsWith(searchTerm))
                    .OrderByDescending(r => r.Id)
                    .Select(r => new ArticlesViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Date = r.Date,
                        Views = r.Views,
                        PublishState = r.PublishState
                    }).ToList();

                return View(model);
            }
        }

        // GET: Admin/Post
        [Authorize(Roles = "administrator, writer")]
        public ActionResult Post(int? id)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Preview (visible seulement pour rédacteur)", Value = "0", Selected = true });
            items.Add(new SelectListItem { Text = "Non répertoriée", Value = "1" });
            items.Add(new SelectListItem { Text = "En ligne", Value = "2" });
            ViewBag.State = items;

            if (id != null)
            { 
                using (var context = new SiteDbContext())
                {
                    Articles model = context.Articles.Find(id);
                    ViewBag.Edition = true;

                    return View(model);
                }
            }
            else
                return View();
        }

        // POST: Admin/Post
        [Authorize(Roles = "administrator, writer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Post(Articles model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Erreur dans le formulaire");
                return View(model);
            }

            model.Tags.Trim(); // Replace Space
            using (var context = new SiteDbContext())
            {
                if(model.Id == null) //Add
                {
                    model.AuthorId = User.Identity.GetUserId();
                    model.Date = DateTime.Now;
                    context.Articles.Add(model);

                    UpdateSitemapXML();
                }
                else //Update
                {
                    model.EditDate = DateTime.Now;
                    context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                }

                context.SaveChanges();
            }

            //Upload Image
            if (file != null && file.ContentLength > 0 && System.IO.Path.GetExtension(file.FileName) == ".jpg")
            {
                string path = System.IO.Path.Combine(Server.MapPath("~/Images/Uploads/Articles/Header/"), model.Id.ToString());

                WebImage img = new WebImage(file.InputStream); //Resize image
                if (img.Width >= 1280 || img.Height >= 720)
                {
                    if (System.IO.File.Exists(path + "x1280"))
                        System.IO.File.Delete(path + "x1280");

                    img.Resize(1280, 720);
                    img.Save(path + "x1280");

                    if (System.IO.File.Exists(path + "x800"))
                        System.IO.File.Delete(path + "x800");

                    img.Resize(800, 450);
                    img.Save(path + "x800");

                    if (System.IO.File.Exists(path + "x320"))
                        System.IO.File.Delete(path + "x320");

                    img.Resize(320, 180);
                    img.Save(path + "x320");
                }
                else
                {
                    ModelState.AddModelError("", "Erreur, l'image n'a pas été modifier, résolution invalide !");
                    return View(model);
                }
            }

            return RedirectToAction("Articles", "Admin");
        }

        // GET: Admin/DeletePost
        public ActionResult DeletePost(int id)
        {
            using (var context = new SiteDbContext())
            {
                Articles post = context.Articles.Find(id);

                string path = System.IO.Path.Combine(Server.MapPath("~/Images/Uploads/Articles/Header/"), post.Id.ToString());
                if (System.IO.File.Exists(path + "x1280.jpeg"))
                    System.IO.File.Delete(path + "x1280.jpeg");

                if (System.IO.File.Exists(path + "x800.jpeg"))
                    System.IO.File.Delete(path + "x800.jpeg");

                if (System.IO.File.Exists(path + "x320.jpeg"))
                    System.IO.File.Delete(path + "x320.jpeg");

                context.Entry(post).State = System.Data.Entity.EntityState.Deleted;
                context.SaveChanges();
            }

            UpdateSitemapXML();
            return RedirectToAction("Articles", "Admin");
        }

        /*************************************/
        /*              MEMBERS              */
        /*************************************/
        [Authorize(Roles = "administrator")]
        public ActionResult Members()
        {
            var applicationDbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var model = (from u in applicationDbContext.Users
                select new MemberViewModel()
                {
                    Id = u.Id,
                    Name = u.UserName,
                    Email = u.Email,
                    RegistrationDate = u.RegistrationDate,
                    LastLoginDate = u.LastLoginDate
                }).ToList();

            return View(model);
        }


        // POST: Admin/DeleteUserRole
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> DeleteUserRole(string id)
        {
            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            ApplicationUserManager _userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));

            id = HttpUtility.UrlDecode(id);

            var user = await _userManager.FindByIdAsync(id);
            var rolesForUser = await _userManager.GetRolesAsync(id);

            //Delete Role
            using (var transaction = context.Database.BeginTransaction())
            {
                //Delete Role
                if (rolesForUser.Count() > 0)
                {
                    foreach (var item in rolesForUser.ToList())
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user.Id, item);
                    }
                }
                transaction.Commit();
            }

            return Json("Rôle supprimer", JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/ChangeUserRole
        [Authorize(Roles = "administrator")]
        public ActionResult ChangeUserRole(string userId, string roleName)
        {
            userId = HttpUtility.UrlDecode(userId);
            roleName = HttpUtility.UrlDecode(roleName);

            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            ApplicationUserManager _userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));
            _userManager.AddToRole(userId, roleName);

            return RedirectToAction("Members", "Admin");
        }

        // POST: Admin/DeleteUser
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            ApplicationUserManager _userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));

            id = HttpUtility.UrlDecode(id);

            var user = await _userManager.FindByIdAsync(id);
            var logins = user.Logins;
            var rolesForUser = await _userManager.GetRolesAsync(id);

            using (var transaction = context.Database.BeginTransaction())
            {
                //Delete Account
                foreach (var login in logins.ToList())
                {
                    await _userManager.RemoveLoginAsync(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                }

                //Delete Role
                if (rolesForUser.Count() > 0)
                {
                    foreach (var item in rolesForUser.ToList())
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user.Id, item);
                    }
                }

                await _userManager.DeleteAsync(user); //Apply query
                transaction.Commit();
            }

            return RedirectToAction("Members", "Admin");
        }

        /*************************************/
        /*                BAN                */
        /*************************************/
        [Authorize(Roles = "administrator, moderator")]
        public ActionResult BanUser(string userId)
        {
            HttpUtility.UrlDecode(userId);

            using (var db = new SiteDbContext())
            {
                AspNetUserBan newBan = new AspNetUserBan
                {
                    UserId = userId
                };

                db.AspNetUserBan.Add(newBan);
                db.SaveChanges();
            }

            return Redirect(ControllerContext.HttpContext.Request.UrlReferrer.ToString());
        }

        [Authorize(Roles = "administrator, moderator")]
        public ActionResult DebanUser(string userId)
        {
            HttpUtility.UrlDecode(userId);

            using (var context = new SiteDbContext())
            {
                AspNetUserBan deban = context.AspNetUserBan.Find(userId);

                context.Entry(deban).State = System.Data.Entity.EntityState.Deleted;
                context.SaveChanges();
            }

            return Redirect(ControllerContext.HttpContext.Request.UrlReferrer.ToString());
        }
    }
}