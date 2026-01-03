using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;


namespace EcommerceMVC.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Analytics
        public ActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return RedirectToAction("Index", "Home");
            }

            // Dashboard summary data
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var model = new AnalyticsDashboardViewModel
            {
                // Sales Summary
                TodaySales = GetSalesForPeriod(today, today.AddDays(1)),
                ThisMonthSales = GetSalesForPeriod(thisMonth, thisMonth.AddMonths(1)),
                LastMonthSales = GetSalesForPeriod(lastMonth, thisMonth),

                // Order Summary
                TodayOrders = GetOrderCountForPeriod(today, today.AddDays(1)),
                ThisMonthOrders = GetOrderCountForPeriod(thisMonth, thisMonth.AddMonths(1)),
                PendingOrders = db.Orders.Count(o => o.OrderStatus == "Processing" || o.OrderStatus == "Pending"),

                // Customer Summary
                TotalCustomers = db.Customers.Count(c => c.IsActive),
                NewCustomersThisMonth = db.Customers.Count(c => c.CreatedDate >= thisMonth),

                // Inventory Summary
                LowStockCount = db.Products.Count(p => p.IsActive && p.StockQuantity <= p.MinStockLevel),
                OutOfStockCount = db.Products.Count(p => p.IsActive && p.StockQuantity <= 0),
                TotalProducts = db.Products.Count(p => p.IsActive),

                // Review Summary
                PendingReviews = db.ProductReviews.Count(r => !r.IsApproved),
                TotalReviews = db.ProductReviews.Count(r => r.IsApproved),
                AverageRating = db.ProductReviews.Where(r => r.IsApproved).Any() ?
                    db.ProductReviews.Where(r => r.IsApproved).Average(r => (decimal)r.Rating) : 0
            };

            return View(model);
        }

        // GET: Analytics/Sales
        public ActionResult Sales(DateTime? startDate, DateTime? endDate)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return RedirectToAction("Index", "Home");
            }

            // Default to last 30 days
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var salesData = db.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.PaymentStatus == "Paid")
                .Include(o => o.Customer)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var model = new SalesAnalyticsViewModel
            {
                StartDate = start,
                EndDate = end.AddDays(-1),
                Orders = salesData,
                TotalSales = salesData.Sum(o => o.TotalAmount),
                TotalOrders = salesData.Count,
                AverageOrderValue = salesData.Any() ? salesData.Average(o => o.TotalAmount) : 0,

                // Daily sales chart data
                DailySales = salesData
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailySalesData
                    {
                        Date = g.Key,
                        Sales = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList(),

                // Top products
                TopProducts = salesData
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => new { oi.ProductID, oi.Product.ProductName })
                    .Select(g => new TopProductData
                    {
                        ProductName = g.Key.ProductName,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }

        // GET: Analytics/Inventory
        public ActionResult Inventory()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new InventoryAnalyticsViewModel
            {
                LowStockProducts = db.Products
                    .Where(p => p.IsActive && p.StockQuantity <= p.MinStockLevel)
                    .OrderBy(p => p.StockQuantity)
                    .ToList(),

                OutOfStockProducts = db.Products
                    .Where(p => p.IsActive && p.StockQuantity <= 0)
                    .OrderBy(p => p.ProductName)
                    .ToList(),

                RecentTransactions = db.InventoryTransactions
                    .Include(t => t.Product)
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(50)
                    .ToList(),

                StockAlerts = db.StockAlerts
                    .Include(a => a.Product)
                    .Where(a => !a.IsResolved)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToList(),

                InventoryValue = db.Products
                    .Where(p => p.IsActive)
                    .Sum(p => p.StockQuantity * (p.CostPrice ?? p.Price))
            };

            return View(model);
        }

        // GET: Analytics/Customers
        public ActionResult Customers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CustomerAnalyticsViewModel
            {
                TopCustomers = db.Orders
                    .Where(o => o.PaymentStatus == "Paid")
                    .GroupBy(o => new { o.CustomerID, o.Customer.FirstName, o.Customer.LastName, o.Customer.Email })
                    .Select(g => new TopCustomerData
                    {
                        CustomerName = g.Key.FirstName + " " + g.Key.LastName,
                        Email = g.Key.Email,
                        TotalOrders = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount),
                        LastOrderDate = g.Max(o => o.OrderDate)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(20)
                    .ToList(),

                NewCustomers = db.Customers
                    .Where(c => c.CreatedDate >= DateTime.Today.AddDays(-30))
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList(),

                CustomerGrowth = db.Customers
                    .Where(c => c.CreatedDate >= DateTime.Today.AddMonths(-12))
                    .GroupBy(c => new { c.CreatedDate.Year, c.CreatedDate.Month })
                    .Select(g => new CustomerGrowthData
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        NewCustomers = g.Count()
                    })
                    .OrderBy(g => g.Month)
                    .ToList()
            };

            return View(model);
        }

        // GET: Analytics/Reviews
        public ActionResult Reviews()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Manager")
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ReviewAnalyticsViewModel
            {
                RecentReviews = db.ProductReviews
                    .Include(r => r.Product)
                    .Include(r => r.Customer)
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(20)
                    .ToList(),

                PendingReviews = db.ProductReviews
                    .Include(r => r.Product)
                    .Include(r => r.Customer)
                    .Where(r => !r.IsApproved)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToList(),

                TopRatedProducts = db.Products
                    .Where(p => p.IsActive && p.ReviewCount > 0)
                    .OrderByDescending(p => p.AverageRating)
                    .ThenByDescending(p => p.ReviewCount)
                    .Take(10)
                    .ToList(),

                RatingDistribution = db.ProductReviews
                    .Where(r => r.IsApproved)
                    .GroupBy(r => r.Rating)
                    .Select(g => new RatingDistributionData
                    {
                        Rating = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.Rating)
                    .ToList()
            };

            return View(model);
        }

        // Helper methods
        private decimal GetSalesForPeriod(DateTime start, DateTime end)
        {
            return db.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.PaymentStatus == "Paid")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;
        }

        private int GetOrderCountForPeriod(DateTime start, DateTime end)
        {
            return db.Orders
                .Count(o => o.OrderDate >= start && o.OrderDate < end);
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
