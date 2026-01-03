using System.Web.Mvc;
using EcommerceMVC.Models;
using System.Linq;

namespace EcommerceMVC.Controllers
{
    public class HomeController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Home
        public ActionResult Index()
        {
            // Get some statistics for the dashboard
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.ActiveProducts = db.Products.Count(p => p.IsActive);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "E-commerce Management System - .NET Framework 4.8 MVC";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
