using System.Linq;
using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Text.Json;


namespace EcommerceMVC.Controllers
{
    [AllowAnonymous]
    public class CartController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        private ShoppingCart GetCart()
        {
            string cartData = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartData) ? null : JsonSerializer.Deserialize<ShoppingCart>(cartData);
            if (cart == null)
            {
                cart = new ShoppingCart();
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
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
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                TempData["Success"] = $"{product.ProductName} added to cart!";

                // If AJAX request, return JSON with cart count
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
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
            return Json(new { count = cart.GetItemCount() });
        }

        // POST: Cart/Update
        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            return RedirectToAction("Index");
        }

        // POST: Cart/Remove
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            TempData["Success"] = "Item removed from cart";
            return RedirectToAction("Index");
        }

        // GET: Cart/Clear
        public ActionResult Clear()
        {
            var cart = GetCart();
            cart.Clear();
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            TempData["Success"] = "Cart cleared";
            return RedirectToAction("Index");
        }

        // GET: Cart/Count (for AJAX)
        public JsonResult Count()
        {
            var cart = GetCart();
            return Json(new { count = cart.GetItemCount() });
        }


    }
}
