using System.Linq;
using System.Web.Mvc;
using EcommerceMVC.Models;

namespace EcommerceMVC.Controllers
{
    [AllowAnonymous]
    public class CartController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        private ShoppingCart GetCart()
        {
            var cart = Session["Cart"] as ShoppingCart;
            if (cart == null)
            {
                cart = new ShoppingCart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // GET: Cart
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: Cart/Add
        [HttpPost]
        public ActionResult Add(int productId, int quantity = 1)
        {
            var product = db.Products.Find(productId);
            if (product != null && product.IsActive)
            {
                var cart = GetCart();
                cart.AddItem(product, quantity);
                Session["Cart"] = cart;

                TempData["Success"] = $"{product.ProductName} added to cart!";
                
                // If AJAX request, return JSON with cart count
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, cartCount = cart.GetItemCount(), message = $"{product.ProductName} added to cart!" });
                }
            }

            return RedirectToAction("Index", "Shop");
        }

        // GET: Cart/Count - For updating cart counter
        public ActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new { count = cart.GetItemCount() }, JsonRequestBehavior.AllowGet);
        }

        // POST: Cart/Update
        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // POST: Cart/Remove
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            Session["Cart"] = cart;

            TempData["Success"] = "Item removed from cart";
            return RedirectToAction("Index");
        }

        // GET: Cart/Clear
        public ActionResult Clear()
        {
            var cart = GetCart();
            cart.Clear();
            Session["Cart"] = cart;

            TempData["Success"] = "Cart cleared";
            return RedirectToAction("Index");
        }

        // GET: Cart/Count (for AJAX)
        public JsonResult Count()
        {
            var cart = GetCart();
            return Json(new { count = cart.GetItemCount() }, JsonRequestBehavior.AllowGet);
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
