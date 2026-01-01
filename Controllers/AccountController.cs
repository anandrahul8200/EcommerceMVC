using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EcommerceMVC.Models;
using EcommerceMVC.Helpers;

namespace EcommerceMVC.Controllers
{
    public class AccountController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Find user by username
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);

                if (user != null && PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    // Update last login date
                    user.LastLoginDate = DateTime.Now;
                    db.SaveChanges();

                    // Create authentication ticket
                    FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

                    // Store user info in session
                    Session["UserID"] = user.UserID;
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.Role;
                    Session["UserFullName"] = user.FullName;

                    // Redirect to return URL or home
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new EcommerceContext())
                    {
                        // Check if user already exists
                        if (db.Users.Any(u => u.Email == model.Email || u.Username == model.Email))
                        {
                            ModelState.AddModelError("", "An account with this email already exists.");
                            return View(model);
                        }

                        // Create new user
                        var newUser = new User
                        {
                            Username = model.Email,
                            Email = model.Email,
                            FirstName = model.FirstName ?? "",
                            LastName = model.LastName ?? "",
                            PasswordHash = PasswordHelper.HashPassword(model.Password),
                            Role = "Customer", // Proper Customer role for e-commerce users
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now
                        };

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        // Create corresponding customer record
                        var customer = new Customer
                        {
                            FirstName = model.FirstName ?? "",
                            LastName = model.LastName ?? "",
                            Email = model.Email,
                            Phone = model.Phone ?? "",
                            CustomerType = "Regular",
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            LoyaltyPoints = 0
                        };

                        db.Customers.Add(customer);
                        db.SaveChanges();

                        // Auto-login the new user
                        FormsAuthentication.SetAuthCookie(newUser.Username, false);
                        Session["UserID"] = newUser.UserID;
                        Session["Username"] = newUser.Username;
                        Session["UserRole"] = newUser.Role;
                        Session["UserFullName"] = newUser.FullName;

                        TempData["Success"] = "Account created successfully! Welcome to our store.";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    // Handle Entity Framework validation errors specifically
                    var validationErrors = new List<string>();
                    
                    foreach (var validationResult in ex.EntityValidationErrors)
                    {
                        var entityName = validationResult.Entry.Entity.GetType().Name;
                        foreach (var error in validationResult.ValidationErrors)
                        {
                            validationErrors.Add($"{entityName}.{error.PropertyName}: {error.ErrorMessage}");
                        }
                    }
                    
                    var validationDetails = string.Join(" | ", validationErrors);
                    System.Diagnostics.Debug.WriteLine("Validation Errors: " + validationDetails);
                    
                    ModelState.AddModelError("", "Registration failed due to validation errors:");
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                catch (Exception ex)
                {
                    // Drill down through all inner exceptions to find the root cause
                    var currentEx = ex;
                    var errorMessages = new List<string>();
                    
                    while (currentEx != null)
                    {
                        errorMessages.Add($"Level {errorMessages.Count + 1}: {currentEx.GetType().Name} - {currentEx.Message}");
                        currentEx = currentEx.InnerException;
                    }
                    
                    var fullErrorDetails = string.Join(" | ", errorMessages);
                    System.Diagnostics.Debug.WriteLine("Registration Error Chain: " + fullErrorDetails);
                    System.Diagnostics.Debug.WriteLine("Full Exception: " + ex.ToString());
                    
                    // Show the deepest (most specific) error message
                    var rootCause = errorMessages.LastOrDefault() ?? "Unknown error";
                    ModelState.AddModelError("", $"Registration failed: {rootCause}");
                    
                    // Also show the full chain for debugging
                    ModelState.AddModelError("", $"Error chain: {fullErrorDetails}");
                }
            }

            return View(model);
        }
    }
}
