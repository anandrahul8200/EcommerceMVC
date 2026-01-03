using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;


namespace EcommerceMVC.Controllers
{
    public class ReviewsController : Controller
    {
        private EcommerceContext db = new EcommerceContext();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewsController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Reviews/Product/5
        public ActionResult Product(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var reviews = db.ProductReviews
                .Include(r => r.Customer)
                .Where(r => r.ProductID == id && r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            ViewBag.Product = product;
            ViewBag.CanReview = CanUserReviewProduct(id);

            return View(reviews);
        }

        // GET: Reviews/Create/5
        public ActionResult Create(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (!CanUserReviewProduct(productId))
            {
                TempData["Error"] = "You can only review products you have purchased.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var model = new ProductReview
            {
                ProductID = productId
            };

            ViewBag.Product = product;
            return View(model);
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductReview review)
        {
            if (!CanUserReviewProduct(review.ProductID))
            {
                TempData["Error"] = "You can only review products you have purchased.";
                return RedirectToAction("Details", "Products", new { id = review.ProductID });
            }

            if (ModelState.IsValid)
            {
                var userIdString = _httpContextAccessor.HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Find customer by user email
                var user = db.Users.Find(Convert.ToInt32(userIdString));
                var customer = db.Customers.FirstOrDefault(c => c.Email == user.Email);

                if (customer == null)
                {
                    TempData["Error"] = "Customer profile not found.";
                    return RedirectToAction("Details", "Products", new { id = review.ProductID });
                }

                // Check if user already reviewed this product
                var existingReview = db.ProductReviews
                    .FirstOrDefault(r => r.ProductID == review.ProductID && r.CustomerID == customer.CustomerID);

                if (existingReview != null)
                {
                    TempData["Error"] = "You have already reviewed this product.";
                    return RedirectToAction("Details", "Products", new { id = review.ProductID });
                }

                // Check if this is a verified purchase
                var hasPurchased = db.Orders
                    .Include(o => o.OrderItems)
                    .Any(o => o.CustomerID == customer.CustomerID &&
                             o.OrderItems.Any(oi => oi.ProductID == review.ProductID) &&
                             o.PaymentStatus == "Paid");

                review.CustomerID = customer.CustomerID;
                review.IsVerifiedPurchase = hasPurchased;
                review.IsApproved = true; // Auto-approve for now, can add moderation later
                review.CreatedDate = DateTime.Now;
                review.ModifiedDate = DateTime.Now;

                db.ProductReviews.Add(review);
                await db.SaveChangesAsync();

                // Update product rating summary
                await UpdateProductRatingSummary(review.ProductID);

                TempData["Success"] = "Thank you for your review!";
                return RedirectToAction("Details", "Products", new { id = review.ProductID });
            }

            var product = db.Products.Find(review.ProductID);
            ViewBag.Product = product;
            return View(review);
        }

        // POST: Reviews/Vote
        [HttpPost]
        public async Task<JsonResult> Vote(int reviewId, bool isHelpful)
        {
            var userIdString = _httpContextAccessor.HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { success = false, message = "Please log in to vote." });
            }

            try
            {
                var user = db.Users.Find(Convert.ToInt32(userIdString));
                var customer = db.Customers.FirstOrDefault(c => c.Email == user.Email);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer profile not found." });
                }

                // Check if user already voted on this review
                var existingVote = db.ReviewVotes
                    .FirstOrDefault(v => v.ReviewID == reviewId && v.CustomerID == customer.CustomerID);

                if (existingVote != null)
                {
                    // Update existing vote
                    existingVote.IsHelpful = isHelpful;
                }
                else
                {
                    // Create new vote
                    var vote = new ReviewVote
                    {
                        ReviewID = reviewId,
                        CustomerID = customer.CustomerID,
                        IsHelpful = isHelpful,
                        CreatedDate = DateTime.Now
                    };
                    db.ReviewVotes.Add(vote);
                }

                await db.SaveChangesAsync();

                // Update review vote counts
                await UpdateReviewVoteCounts(reviewId);

                return Json(new { success = true, message = "Thank you for your feedback!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while processing your vote." });
            }
        }

        // GET: Reviews/Manage (Admin only)
        [Authorize]
        public ActionResult Manage()
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }

            var reviews = db.ProductReviews
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return View(reviews);
        }

        // POST: Reviews/Approve/5
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Approve(int id)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }

            var review = db.ProductReviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            review.IsApproved = true;
            review.ModifiedDate = DateTime.Now;
            await db.SaveChangesAsync();

            await UpdateProductRatingSummary(review.ProductID);

            TempData["Success"] = "Review approved successfully.";
            return RedirectToAction("Manage");
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }

            var review = db.ProductReviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            var productId = review.ProductID;

            // Delete associated votes first
            var votes = db.ReviewVotes.Where(v => v.ReviewID == id);
            db.ReviewVotes.RemoveRange(votes);

            db.ProductReviews.Remove(review);
            await db.SaveChangesAsync();

            await UpdateProductRatingSummary(productId);

            TempData["Success"] = "Review deleted successfully.";
            return RedirectToAction("Manage");
        }

        private bool CanUserReviewProduct(int productId)
        {
            var userIdString = _httpContextAccessor.HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString)) return false;

            var user = db.Users.Find(Convert.ToInt32(userIdString));
            if (user == null) return false;

            var customer = db.Customers.FirstOrDefault(c => c.Email == user.Email);
            if (customer == null) return false;

            // Check if customer has purchased this product
            return db.Orders
                .Include(o => o.OrderItems)
                .Any(o => o.CustomerID == customer.CustomerID &&
                         o.OrderItems.Any(oi => oi.ProductID == productId) &&
                         o.PaymentStatus == "Paid");
        }

        private async Task UpdateProductRatingSummary(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null) return;

            var approvedReviews = db.ProductReviews
                .Where(r => r.ProductID == productId && r.IsApproved)
                .ToList();

            if (approvedReviews.Any())
            {
                product.AverageRating = (decimal)approvedReviews.Average(r => r.Rating);
                product.ReviewCount = approvedReviews.Count;
            }
            else
            {
                product.AverageRating = 0;
                product.ReviewCount = 0;
            }

            product.ModifiedDate = DateTime.Now;
            await db.SaveChangesAsync();
        }

        private async Task UpdateReviewVoteCounts(int reviewId)
        {
            var review = db.ProductReviews.Find(reviewId);
            if (review == null) return;

            var votes = db.ReviewVotes.Where(v => v.ReviewID == reviewId).ToList();

            review.HelpfulVotes = votes.Count(v => v.IsHelpful);
            review.UnhelpfulVotes = votes.Count(v => !v.IsHelpful);
            review.ModifiedDate = DateTime.Now;

            await db.SaveChangesAsync();
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
