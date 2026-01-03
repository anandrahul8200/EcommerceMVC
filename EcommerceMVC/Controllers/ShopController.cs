using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EcommerceMVC.Models;

namespace EcommerceMVC.Controllers
{
    [AllowAnonymous]
    public class ShopController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Shop
        [OutputCache(Duration = 300, VaryByParam = "search;categoryId;minPrice;maxPrice;page")]
        public ActionResult Index(string search, int? categoryId, decimal? minPrice, decimal? maxPrice, int? page)
        {
            // Performance: Include Category to avoid N+1 queries
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Search filter - Use StartsWith for better performance
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => 
                    p.ProductName.StartsWith(search) || 
                    p.SKU.StartsWith(search) ||
                    p.ProductName.Contains(search) ||
                    p.Description.Contains(search));
                ViewBag.Search = search;
            }

            // Category filter
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
                ViewBag.CategoryId = categoryId;
            }

            // Price range filter
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
                ViewBag.MinPrice = minPrice;
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice;
            }

            // Get categories for filter dropdown (cached)
            ViewBag.Categories = db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToList();

            // Performance: Add pagination
            int pageSize = 20;
            int pageNumber = page ?? 1;
            
            var totalCount = products.Count();
            var productList = products
                .OrderBy(p => p.ProductName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ResultCount = totalCount;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View(productList);
        }

        // GET: Shop/Details/5
        public ActionResult Details(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
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
