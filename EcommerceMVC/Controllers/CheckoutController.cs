using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using EcommerceMVC.Models;
using EcommerceMVC.Helpers;
using EcommerceMVC.Services;

namespace EcommerceMVC.Controllers
{
    [AllowAnonymous]
    public class CheckoutController : Controller
    {
        private EcommerceContext db = new EcommerceContext();
        private EmailService emailService = new EmailService();
        private InventoryService inventoryService = new InventoryService();
        private StripePaymentService stripeService = new StripePaymentService();

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

        // GET: Checkout
        public ActionResult Index()
        {
            var cart = GetCart();
            
            if (cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Shop");
            }

            ViewBag.Cart = cart;
            
            // If user is already logged in, go directly to checkout form
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Details");
            }
            
            // Otherwise, show checkout options
            return RedirectToAction("Options");
        }

        // GET: Checkout/Options
        public ActionResult Options()
        {
            var cart = GetCart();
            
            if (cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Shop");
            }

            ViewBag.Cart = cart;
            return View(new CheckoutOptionViewModel());
        }

        // POST: Checkout/ProcessOption
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOption(CheckoutOptionViewModel model)
        {
            var cart = GetCart();
            ViewBag.Cart = cart;

            if (cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Shop");
            }

            try
            {
                switch (model.CheckoutType)
                {
                    case "login":
                        return ProcessLogin(model);
                    case "register":
                        return ProcessRegistration(model);
                    case "guest":
                        return RedirectToAction("Details");
                    default:
                        ModelState.AddModelError("", "Please select a checkout option");
                        return View("Options", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View("Options", model);
            }
        }

        private ActionResult ProcessLogin(CheckoutOptionViewModel model)
        {
            if (string.IsNullOrEmpty(model.LoginUsername) || string.IsNullOrEmpty(model.LoginPassword))
            {
                ModelState.AddModelError("", "Please enter username and password");
                return View("Options", model);
            }

            // Find user by username
            var user = db.Users.FirstOrDefault(u => u.Username == model.LoginUsername && u.IsActive);

            if (user != null && PasswordHelper.VerifyPassword(model.LoginPassword, user.PasswordHash))
            {
                // Update last login date
                user.LastLoginDate = DateTime.Now;
                db.SaveChanges();

                // Create authentication ticket
                FormsAuthentication.SetAuthCookie(user.Username, false);

                // Store user info in session
                Session["UserID"] = user.UserID;
                Session["Username"] = user.Username;
                Session["UserRole"] = user.Role;
                Session["UserFullName"] = user.FullName;

                return RedirectToAction("Details");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View("Options", model);
            }
        }

        private ActionResult ProcessRegistration(CheckoutOptionViewModel model)
        {
            if (string.IsNullOrEmpty(model.RegisterUsername) || string.IsNullOrEmpty(model.RegisterEmail) || 
                string.IsNullOrEmpty(model.RegisterPassword))
            {
                ModelState.AddModelError("", "Please fill in all required registration fields");
                return View("Options", model);
            }

            if (model.RegisterPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View("Options", model);
            }

            // Check if username or email already exists
            if (db.Users.Any(u => u.Username == model.RegisterUsername))
            {
                ModelState.AddModelError("", "Username already exists");
                return View("Options", model);
            }

            if (db.Users.Any(u => u.Email == model.RegisterEmail))
            {
                ModelState.AddModelError("", "Email already exists");
                return View("Options", model);
            }

            // Create new user
            var newUser = new User
            {
                Username = model.RegisterUsername,
                Email = model.RegisterEmail,
                FirstName = model.RegisterFirstName ?? "",
                LastName = model.RegisterLastName ?? "",
                PasswordHash = PasswordHelper.HashPassword(model.RegisterPassword),
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            // Log in the new user
            FormsAuthentication.SetAuthCookie(newUser.Username, false);

            // Store user info in session
            Session["UserID"] = newUser.UserID;
            Session["Username"] = newUser.Username;
            Session["UserRole"] = newUser.Role;
            Session["UserFullName"] = newUser.FullName;

            return RedirectToAction("Details");
        }

        // GET: Checkout/Details
        public ActionResult Details()
        {
            var cart = GetCart();
            
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Shop");
            }

            ViewBag.Cart = cart;
            
            // Pre-populate form with logged-in user's information
            var model = new CheckoutViewModel();
            
            try
            {
                var userId = Session["UserID"];
                
                if (userId != null)
                {
                    var userIdInt = Convert.ToInt32(userId);
                    var user = db.Users.Find(userIdInt);
                    
                    if (user != null)
                    {
                        model.Email = user.Email ?? "";
                        model.FirstName = user.FirstName ?? "";
                        model.LastName = user.LastName ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue - user can still fill out the form manually
                System.Diagnostics.Debug.WriteLine("Error loading user data for checkout: " + ex.Message);
            }
            
            return View(model);
        }

        // POST: Checkout/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details(CheckoutViewModel model)
        {
            var cart = GetCart();
            ViewBag.Cart = cart;

            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                ModelState.AddModelError("", "Your cart is empty");
                return RedirectToAction("Index", "Shop");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate cart items exist and have valid data
                if (cart?.Items == null)
                {
                    ModelState.AddModelError("", "Cart is invalid");
                    return View(model);
                }

                foreach (var item in cart.Items)
                {
                    if (item == null)
                    {
                        ModelState.AddModelError("", "Invalid cart item found");
                        return View(model);
                    }
                    
                    var product = db.Products.Find(item.ProductID);
                    if (product == null)
                    {
                        ModelState.AddModelError("", $"Product not found: {item.ProductID}");
                        return View(model);
                    }
                }

                // Use the email from the form (user's choice)
                string orderEmail = model.Email;

                if (string.IsNullOrEmpty(orderEmail))
                {
                    ModelState.AddModelError("", "Email is required");
                    return View(model);
                }

                // If user wants to create account and isn't already logged in
                if (model.CreateAccount && Session["UserID"] == null && !string.IsNullOrEmpty(model.Password))
                {
                    // Check if user already exists
                    if (!db.Users.Any(u => u.Email == model.Email || u.Username == model.Email))
                    {
                        // Create new user account
                        var newUser = new User
                        {
                            Username = model.Email, // Use email as username
                            Email = model.Email,
                            FirstName = model.FirstName ?? "",
                            LastName = model.LastName ?? "",
                            PasswordHash = PasswordHelper.HashPassword(model.Password),
                            Role = "Customer",
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now
                        };

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        // Log in the new user
                        FormsAuthentication.SetAuthCookie(newUser.Username, false);
                        Session["UserID"] = newUser.UserID;
                        Session["Username"] = newUser.Username;
                        Session["UserRole"] = newUser.Role;
                        Session["UserFullName"] = newUser.FullName;
                    }
                }

                // Create or find customer using the provided email
                var customer = db.Customers.FirstOrDefault(c => c.Email == orderEmail);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        FirstName = model.FirstName ?? "",
                        LastName = model.LastName ?? "",
                        Email = orderEmail, // Use the email from form
                        Phone = model.Phone ?? "",
                        CustomerType = "Regular",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        LoyaltyPoints = 0
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();
                }

                // Create address
                var address = new Address
                {
                    CustomerID = customer.CustomerID,
                    AddressType = "Both",
                    Street1 = model.Street ?? "",
                    City = model.City ?? "",
                    State = model.State ?? "",
                    ZipCode = model.ZipCode ?? "",
                    Country = model.Country ?? "USA",
                    IsDefault = true,
                    CreatedDate = DateTime.Now
                };
                db.Addresses.Add(address);
                db.SaveChanges();

                // Calculate totals safely
                var subtotal = cart.GetTotal();
                var taxAmount = subtotal * 0.085m; // 8.5% tax
                var shippingAmount = subtotal >= 100 ? 0 : 9.99m;
                var totalAmount = subtotal + taxAmount + shippingAmount;

                // Create order first
                var orderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerID = customer.CustomerID,
                    ShippingAddressID = address.AddressID,
                    BillingAddressID = address.AddressID,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Pending", // Will update after payment
                    PaymentStatus = "Pending", // Will update after payment
                    SubTotal = subtotal,
                    TaxAmount = taxAmount,
                    ShippingAmount = shippingAmount,
                    DiscountAmount = 0,
                    TotalAmount = totalAmount,
                    Notes = model.Notes ?? "",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
                
                db.Orders.Add(order);
                db.SaveChanges();

                // Now process payment with Stripe
                var paymentRequest = new Services.PaymentRequest
                {
                    Token = model.StripeToken ?? "",
                    Amount = order.TotalAmount,
                    OrderNumber = orderNumber,
                    OrderId = order.OrderID,
                    CustomerEmail = model.Email ?? "",
                    Description = "E-commerce Order"
                };

                var paymentResult = await stripeService.ProcessPaymentAsync(paymentRequest);

                if (!paymentResult.Success)
                {
                    // Payment failed - delete the order
                    db.Orders.Remove(order);
                    db.SaveChanges();
                    
                    ModelState.AddModelError("", $"Payment failed: {paymentResult.Message}");
                    return View(model);
                }

                // Payment successful - update order status
                order.OrderStatus = "Processing";
                order.PaymentStatus = "Paid";
                db.SaveChanges();

                // Create order items
                foreach (var item in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        Discount = 0,
                        TaxRate = 8.5m,
                        CreatedDate = DateTime.Now
                    };
                    db.OrderItems.Add(orderItem);
                }
                db.SaveChanges();

                // Create payment record
                var payment = new Payment
                {
                    OrderID = order.OrderID,
                    PaymentMethod = "Credit Card",
                    PaymentDate = DateTime.Now,
                    Amount = order.TotalAmount,
                    TransactionID = paymentResult.TransactionId ?? "",
                    PaymentStatus = "Completed",
                    CreatedDate = DateTime.Now
                };
                db.Payments.Add(payment);
                db.SaveChanges();

                // Clear cart
                cart.Clear();
                Session["Cart"] = cart;

                // Process inventory reduction
                await inventoryService.ProcessOrderStockReductionAsync(order, Session["Username"]?.ToString() ?? "System");

                // Send order confirmation email
                await emailService.SendOrderConfirmationAsync(order);

                // Redirect to confirmation
                return RedirectToAction("Confirmation", new { orderId = order.OrderID });
            }
            catch (Exception ex)
            {
                // Log detailed error information
                System.Diagnostics.Debug.WriteLine("Checkout Error: " + ex.ToString());
                
                var errorMessage = "An error occurred while processing your order: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " Inner: " + ex.InnerException.Message;
                }
                
                ModelState.AddModelError("", errorMessage);
                return View(model);
            }
        }

        // GET: Checkout/Confirmation
        public ActionResult Confirmation(int orderId)
        {
            var order = db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .Include("OrderItems.Product")
                .FirstOrDefault(o => o.OrderID == orderId);
                
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                emailService?.Dispose();
                inventoryService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
