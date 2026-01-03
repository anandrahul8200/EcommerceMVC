using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace EcommerceMVC.Controllers
{
    public class OrdersController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Orders
        public ActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserID");

            IQueryable<Order> ordersQuery = db.Orders.Include(o => o.Customer);

            // Filter orders based on user role
            if (userRole == "Admin" || userRole == "Manager")
            {
                // Admin/Manager can see all orders
                ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);
            }
            else
            {
                // Regular customers can only see their own orders
                if (userId != null)
                {
                    // Find the user's email to match with customer records
                    int userIdInt = Convert.ToInt32(userId);
                    var user = db.Users.Find(userIdInt);

                    if (user != null)
                    {
                        ordersQuery = ordersQuery
                            .Where(o => o.Customer.Email == user.Email)
                            .OrderByDescending(o => o.OrderDate);
                    }
                    else
                    {
                        // If user not found, return empty list
                        return View(new List<Order>());
                    }
                }
                else
                {
                    // If no user ID in session, return empty list
                    return View(new List<Order>());
                }
            }

            // Limit results to improve performance (last 50 orders)
            var orders = ordersQuery.Take(50).ToList();

            return View(orders);
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserID");

            IQueryable<Order> orderQuery = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems.Select(oi => oi.Product));

            // Apply authorization filter
            if (userRole == "Admin" || userRole == "Manager")
            {
                // Admin/Manager can see any order
                orderQuery = orderQuery.Where(o => o.OrderID == id);
            }
            else
            {
                // Regular customers can only see their own orders
                if (userId != null)
                {
                    // Find the user's email to match with customer records
                    int userIdInt = Convert.ToInt32(userId);
                    var user = db.Users.Find(userIdInt);

                    if (user != null)
                    {
                        orderQuery = orderQuery.Where(o => o.OrderID == id && o.Customer.Email == user.Email);
                    }
                    else
                    {
                        return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                    }
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }

            Order order = orderQuery.FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
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
