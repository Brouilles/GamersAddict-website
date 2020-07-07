using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using GamersAddict.Models;

namespace GamersAddict.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        public async Task<ActionResult> RemoveLogin()
        {
            var id = User.Identity.GetUserId();

            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            ApplicationUserManager _userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));

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

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/ChangeEmail
        public ActionResult ChangeEmail()
        {
            ChangeEmailViewModel model = new ChangeEmailViewModel();
            var userId = User.Identity.GetUserId();

            //Get Email
            var applicationDbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            model.Email = applicationDbContext.Users
                .Where(r => r.Id == userId)
                .Select(r => r.Email).ToArray()[0];

            return View(model);
        }

        //
        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Update Account
                var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                ApplicationUserManager _userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));
                var user = _userManager.FindByName(User.Identity.Name);

                var oldEmail = user.Email;
                user.Email = model.Email;
                _userManager.Update(user);
                HttpContext.GetOwinContext().Get<ApplicationDbContext>().SaveChanges();

                //Update Newsletter
                using (var db = new SiteDbContext())
                {
                    var ifEmailExist = db.NewsletterRegistration
                        .Any(r => r.Email.Equals(oldEmail));

                    if (ifEmailExist)
                    {
                        NewsletterRegistration newsletterRegistration = db.NewsletterRegistration
                            .Where(r => r.Email == oldEmail)
                            .Select(r => r).FirstOrDefault();

                        newsletterRegistration.Email = model.Email;

                        db.Entry(newsletterRegistration).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                //Update YouTube Alert
                using (var db = new SiteDbContext())
                {
                    var ifEmailExist = db.YouTubeAlertRegistration
                        .Any(r => r.Email.Equals(oldEmail));

                    if (ifEmailExist)
                    {
                        YouTubeAlertRegistration youTubeAlertRegistration = db.YouTubeAlertRegistration
                            .Where(r => r.Email == oldEmail)
                            .Select(r => r).FirstOrDefault();

                        youTubeAlertRegistration.Email = model.Email;

                        db.Entry(youTubeAlertRegistration).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index", new { Message = "Adresse Email modifier avec succès !" });
            }

            return View(model);
        }

        //
        // GET: /Manage/ChangeNewsletter
        public ActionResult ChangeNewsletter()
        {
            using (var db = new SiteDbContext())
            {
                //Get Username
                var pseudo = User.Identity.GetUserName();

                //Get Email
                var applicationDbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                var email = applicationDbContext.Users
                    .Where(r => r.UserName == pseudo)
                    .Select(r => r.Email).ToArray()[0];

                ViewBag.email = email;

                //Verify if is register
                var ifEmailExist = db.NewsletterRegistration
                    .Any(r => r.Email.Equals(email));

                if (ifEmailExist)
                {
                    var userData = db.NewsletterRegistration.Where(r => r.Email.Equals(email)).FirstOrDefault();
                    ViewBag.register = "true";
                    ViewBag.secretKey = userData.SecretKey;

                    return View();
                }
                else
                {
                    ViewBag.register = "false";
                    NewsletterRegistration newsletterModel = new NewsletterRegistration();
                    newsletterModel.Email = email;

                    return View(newsletterModel);
                }
            }
        }

        //
        // GET: /Manage/ChangeYouTubeAlert
        public ActionResult ChangeYouTubeAlert()
        {
            using (var db = new SiteDbContext())
            {
                //Get Username
                var pseudo = User.Identity.GetUserName();

                //Get Email
                var applicationDbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                var email = applicationDbContext.Users
                    .Where(r => r.UserName == pseudo)
                    .Select(r => r.Email).ToArray()[0];

                ViewBag.email = email;

                //Verify if is register
                var ifEmailExist = db.YouTubeAlertRegistration
                    .Any(r => r.Email.Equals(email));

                if (ifEmailExist)
                {
                    var userData = db.YouTubeAlertRegistration.Where(r => r.Email.Equals(email)).FirstOrDefault();
                    ViewBag.register = "true";
                    ViewBag.secretKey = userData.SecretKey;

                    return View();
                }
                else
                {
                    ViewBag.register = "false";
                    YouTubeAlertRegistration newsletterModel = new YouTubeAlertRegistration();
                    newsletterModel.Email = email;

                    return View(newsletterModel);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YouTubeAlertSubscribe(YouTubeAlertRegistration model)
        {
            using (var db = new SiteDbContext())
            {
                var ifEmailExist = db.YouTubeAlertRegistration
                    .Any(r => r.Email.Equals(model.Email));

                if (!ifEmailExist)
                {

                    YouTubeAlertRegistration newRegistration = new YouTubeAlertRegistration
                    {
                        Email = model.Email,
                        SecretKey = Guid.NewGuid().ToString()
                    };

                    db.YouTubeAlertRegistration.Add(newRegistration);
                    db.SaveChanges();

                    return Json("Inscription effectuée !", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("L'adresse email est déjà inscrite !", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult YouTubeAlertUnsubscribe(string email, string guid)
        {
            email = HttpUtility.UrlDecode(email);
            guid = HttpUtility.UrlDecode(guid);

            using (var db = new SiteDbContext())
            {
                var ifEmailExist = db.YouTubeAlertRegistration
                    .Any(r => r.Email.Equals(email));

                if (ifEmailExist)
                {
                    var userData = db.YouTubeAlertRegistration.Where(r => r.Email.Equals(email)).FirstOrDefault();
                    if (userData.Email == email && userData.SecretKey == guid)
                    {
                        YouTubeAlertRegistration newsletterAccount = db.YouTubeAlertRegistration.Find(userData.Id);
                        db.Entry(newsletterAccount).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();

                        ViewBag.Message = "Vous êtes maintenant désinscrit aux alertes YouTube de Gamers Addict.";
                    }
                    else
                        ViewBag.Message = "Erreur dans clef secrète.";
                }
                else
                    ViewBag.Message = "Vous n'êtes pas inscrit à la newsletter, désinscription impossible.";
            }
            return View();
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}