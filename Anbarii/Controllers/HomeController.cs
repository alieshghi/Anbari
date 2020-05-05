using Anbarii.Classes;
using Anbarii.Classes.Values;
using Anbarii.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Anbarii.Controllers
{
    public class HomeController : Controller
    {
        public const string SController = "Home";
        public const string SIndex = "Index";
        public const string SRegister = "Register";
        private AnbariiEntities db = new AnbariiEntities();
        // GET: Home
        [HttpGet]
        public ActionResult Index()
        {
            if (Members.Login())
            {
                return RedirectToAction(string.Empty, PortalController.SController);
            }
            ViewBag.error = string.Empty;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string Email, string Password)
        {
            List<object> _return = Members.Login(Email, Password);
            ViewBag.error = (string)_return[0];
            if (Members.Login())
            {
                return RedirectToAction(string.Empty, PortalController.SController);
            }
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            if (Members.Login())
            {
                return RedirectToAction(string.Empty, PortalController.SController);
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user, int Role)
        {
            if (Members.Login())
            {
                return RedirectToAction(string.Empty, PortalController.SController);
            }
            if (user == null || (!RolesInt.Anbar.Equals(Role) && !RolesInt.Tajer.Equals(Role)))
                return View();
            List<string> Errorlist = user.Validate();
            if (!Errorlist.Any())
            {
                user.RoleID = Role;
                user.Password = user.Password.GetMd5Hash();
                user.Token = "";
                user.TryCount = 0;
                user.LastLoginDate = DateTime.Now;
                user.Edite = false;
                user.Validate = false;
                db.Users.Add(user);
                db.SaveChanges();
                user.Token = user.ID.ToString(CultureInfo.CurrentCulture).GetMd5Hash();
                db.SaveChanges();
                ViewBag.error = "ثبت نام انجام شد";
                return View(SIndex);
            }
            else
                foreach (string item in Errorlist)
                    ModelState.AddModelError(item, item);

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckEmail(string Email)
        {
            if (!string.IsNullOrEmpty(Email))
                if (db.Users.Where(i => i.Email.Equals(Email, StringComparison.OrdinalIgnoreCase)).Any())
                    return Json(true);
            return Json(false);
        }
    }
}