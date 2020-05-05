using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Anbarii.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        [HttpGet]
        public ActionResult Index(HandleErrorInfo exception)
        {
            return View("Error",exception);
        }
        [HttpGet]
        public ActionResult NotFound()
        {
            return View();
        }
    }
}