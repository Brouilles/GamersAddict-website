using GamersAddict.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace GamersAddict.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Video()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AjaxContact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                MailMessage email = new MailMessage();
                email.From = new MailAddress("email@gamersaddict.fr");
                email.To.Add(new MailAddress("gaetan.dezeiraud@outlook.com"));

                email.Subject = "[Gamers Addict] -" + model.Title;
                email.Body = $"Ceci est un message venant du formulaire de contact du site Gamers Addict.\nNom: { model.Pseudo }\nEmail: { model.Email }(cliquer sur l'adresse email pour répondre)\n\nTitre: { model.Title }\n{ model.Text }";

                SmtpClient client = new SmtpClient();
                client.Send(email);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewsletterSubscribe(NewsletterRegistration model)
        {
            using (var db = new SiteDbContext())
            {
                var ifEmailExist = db.NewsletterRegistration
                    .Any(r => r.Email.Equals(model.Email));

                if (!ifEmailExist)
                {

                    NewsletterRegistration newRegistration = new NewsletterRegistration
                    {
                        Email = model.Email,
                        SecretKey = Guid.NewGuid().ToString()
                    };

                    db.NewsletterRegistration.Add(newRegistration);
                    db.SaveChanges();

                    return Json("Inscription effectuée !", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("L'adresse email est déjà inscrite !", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult NewsletterUnsubscribe(string email, string guid)
        {
            email = HttpUtility.UrlDecode(email);
            guid = HttpUtility.UrlDecode(guid);

            using (var db = new SiteDbContext())
            {
                var ifEmailExist = db.NewsletterRegistration
                    .Any(r => r.Email.Equals(email));

                if (ifEmailExist)
                {
                    var userData = db.NewsletterRegistration.Where(r => r.Email.Equals(email)).FirstOrDefault();
                    if (userData.Email == email && userData.SecretKey == guid)
                    {
                        NewsletterRegistration newsletterAccount = db.NewsletterRegistration.Find(userData.Id);
                        db.Entry(newsletterAccount).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();

                        ViewBag.Message = "Vous êtes maintenant désinscrit de la newsletter de Gamers Addict.";
                    }
                    else
                        ViewBag.Message = "Erreur dans clef secrète.";
                }
                else
                    ViewBag.Message = "Vous n'êtes pas inscrit à la newsletter, désinscription impossible.";
            }
            return View();
        }
    }
}