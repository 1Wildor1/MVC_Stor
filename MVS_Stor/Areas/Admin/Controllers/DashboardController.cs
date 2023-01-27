using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVS_Stor.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Admin")]
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}