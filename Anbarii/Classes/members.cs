using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Mail;
using System.Data.Entity;
using Anbarii.Models;
using Anbarii.Classes.Values;
using System.Globalization;

namespace Anbarii.Classes
{
    /// <summary>
    ///کاربران
    /// </summary>
    public static class Members
    {
        private static AnbariiEntities db = new AnbariiEntities();
        private const string _Redirect = "Redirect";
        private const string _User = "User";
        private const string _Year = "Year";
        /// <summary>
        /// لینک بازیابی
        /// </summary>
        public static string Redirect
        {
            get { return (HttpContext.Current.Session[_Redirect] == null) ? string.Empty : HttpContext.Current.Session[_Redirect].ToString(); }
            set { HttpContext.Current.Session[_Redirect] = value; }
        }
        /// <summary>
        ///  کاربر
        /// </summary>
        public static User User
        {
            get { return (HttpContext.Current.Session[_User] == null) ? null : (User)HttpContext.Current.Session[_User]; }
            set { HttpContext.Current.Session[_User] = value; }

        }
        /// <summary>
        ///  سال
        /// </summary>
        public static int Year
        {
            get { return (HttpContext.Current.Session[_Year] == null) ? DateTime.Now.MiladiTopersianYear() : (int)HttpContext.Current.Session[_Year]; }
            set { HttpContext.Current.Session[_Year] = value; }

        }
        /// <summary>
        /// دریافت نام سمت
        /// 1 ادمین
        /// 2 تاجر
        /// 3 انبار
        /// </summary>
        public static string RoleName()
        {
            var resulat = string.Empty;
            resulat = db.Roles.Where(i => i.ID.Equals(User.RoleID)).FirstOrDefault()?.Name;
            return resulat;
        }

        /// <summary>
        /// چک کردن دسترسی
        /// 1 ادمین
        /// 2 تاجر
        /// 3 انبار
        /// </summary>
        /// <param name="roleget">لیست سمت کاربر</param>
        public static bool Login(int[] roleget)
        {
            int pos = Array.IndexOf(roleget, User?.RoleID);
            if (string.IsNullOrEmpty(User?.Email) || pos <= -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static void UpdateUser()
        {
            db.Dispose();
            db = new AnbariiEntities();
            User = db.Users.Where(i => i.ID.Equals(User.ID)).FirstOrDefault();
        }

        /// <summary>
        /// ورود
        /// </summary>
        /// <param name="roleget">لیست سمت کاربر</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static List<object> Login(string Email, string password)
        {
            List<object> _Return = new List<object>(2)
            {
                 string.Empty,
                string.Empty
            };
            try
            {
                password = MD5Hash.GetMd5Hash(password);
                var user = db.Users.Where(i => i.Email.Equals(Email, StringComparison.OrdinalIgnoreCase) && i.Password == password).FirstOrDefault();
                if (user == null)
                {
                    _Return[0] = Strings.IncorrectUsernameAndPassword;
                    _Return[1] = false.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    User = user;
                    //if (members.Redirect != "")
                    //{
                    //     controller.Redirect(members.Redirect.ToString());

                    //}
                    //else
                    //{
                    //     RedirectToAction("", "portal");
                    //}
                    _Return[1] = true.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch
            {

                _Return[0] = Strings.DataBaseConnectionError;
            }
            return _Return;
        }
        /// <summary>
        /// وضعیت
        /// </summary>
        public static bool Login()
        {
            if ((string.IsNullOrEmpty(User?.Email) && User?.RoleID <= 0) || User == null)
            {
                Redirect = HttpContext.Current.Request.RawUrl.ToString(CultureInfo.CurrentCulture);
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        /// <param name="confirm">تایید رمز عبور</param>
        /// <param name="newpass">رمز عبور جدید</param>
        /// <param name="oldpass">رمز عبور قدیم</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static bool Changepass(string oldpass, string newpass, string confirm)
        {
            oldpass = oldpass.GetMd5Hash();
            newpass = newpass.GetMd5Hash();
            confirm = confirm.GetMd5Hash();
            try
            {

                var user = db.Users.Where(i => i.Email.Equals(User.Email, StringComparison.OrdinalIgnoreCase) && i.Password == oldpass).FirstOrDefault();
                if (user != null && confirm == newpass)
                {
                    user.Password = newpass;
                    db.SaveChanges();
                    return true;
                }

            }
            catch { }
            return false;
        }
        /// <summary>
        /// خروج
        /// </summary>
        public static void Logout()
        {
            User = null;
        }
    }
}