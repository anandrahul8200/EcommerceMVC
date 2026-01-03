using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EcommerceMVC.Models;
using Microsoft.Data.SqlClient;

using Microsoft.AspNetCore.Mvc;


namespace EcommerceMVC.Controllers
{
    public class ReportsController : Controller
    {
        private EcommerceContext db = new EcommerceContext();

        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }

        // GET: Reports/LowStock
        public ActionResult LowStock(string warehouse = null)
        {
            var lowStockProducts = new List<LowStockProduct>();

            using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var command = new SqlCommand("sp_GetLowStockProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    
                    // Add parameter
                    command.Parameters.AddWithValue("@WarehouseLocation", 
                        string.IsNullOrEmpty(warehouse) ? (object)DBNull.Value : warehouse);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lowStockProducts.Add(new LowStockProduct
                            {
                                ProductID = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                SKU = reader.GetString(2),
                                CategoryName = reader.GetString(3),
                                WarehouseLocation = reader.GetString(4),
                                QuantityOnHand = reader.GetInt32(5),
                                QuantityReserved = reader.GetInt32(6),
                                AvailableQuantity = reader.GetInt32(7),
                                ReorderLevel = reader.GetInt32(8),
                                ReorderQuantity = reader.GetInt32(9),
                                LastRestockDate = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10)
                            });
                        }
                    }
                }
            }

            ViewBag.Warehouse = warehouse;
            return View(lowStockProducts);
        }

        // GET: Reports/CustomerOrders/5
        public ActionResult CustomerOrders(int id, DateTime? startDate = null, DateTime? endDate = null)
        {
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            var orderHistory = new List<CustomerOrderHistory>();

            using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var command = new SqlCommand("sp_GetCustomerOrderHistory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    
                    command.Parameters.AddWithValue("@CustomerID", id);
                    command.Parameters.AddWithValue("@StartDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate.HasValue ? (object)endDate.Value : DBNull.Value);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderHistory.Add(new CustomerOrderHistory
                            {
                                OrderID = reader.GetInt32(0),
                                OrderNumber = reader.GetString(1),
                                OrderDate = reader.GetDateTime(2),
                                OrderStatus = reader.GetString(3),
                                PaymentStatus = reader.GetString(4),
                                TotalAmount = reader.GetDecimal(5),
                                ItemCount = reader.GetInt32(6),
                                TrackingNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                                ShipmentStatus = reader.IsDBNull(8) ? null : reader.GetString(8)
                            });
                        }
                    }
                }
            }

            ViewBag.Customer = customer;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            
            return View(orderHistory);
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
