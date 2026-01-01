using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EcommerceMVC.Models;

namespace EcommerceMVC.Controllers
{
    public class ProductsController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Products
        [OutputCache(Duration = 180)] // Cache for 3 minutes
        public ActionResult Index()
        {
            var products = db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();
            return View(products);
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductName,SKU,CategoryID,Description,Price,CostPrice,Weight,Dimensions,IsActive,IsFeatured,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.ModifiedDate = DateTime.Now;
                
                db.Products.Add(product);
                db.SaveChanges();
                
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Product product = db.Products.Find(id);
            
            if (product == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.CategoryID = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,SKU,CategoryID,Description,Price,CostPrice,Weight,Dimensions,IsActive,IsFeatured,ImageUrl,CreatedDate")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.ModifiedDate = DateTime.Now;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                
                return RedirectToAction("Index");
            }
            
            ViewBag.CategoryID = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            
            return RedirectToAction("Index");
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
