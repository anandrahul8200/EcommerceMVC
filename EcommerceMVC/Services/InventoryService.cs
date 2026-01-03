using System;
using System.Linq;
using System.Threading.Tasks;
using EcommerceMVC.Models;

namespace EcommerceMVC.Services
{
    public class InventoryService
    {
        private readonly EcommerceContext _db;
        private readonly EmailService _emailService;

        public InventoryService()
        {
            _db = new EcommerceContext();
            _emailService = new EmailService();
        }

        public InventoryService(EcommerceContext context)
        {
            _db = context;
            _emailService = new EmailService();
        }

        /// <summary>
        /// Updates product stock and creates inventory transaction
        /// </summary>
        public async Task<bool> UpdateStockAsync(int productId, int quantity, string transactionType, 
            string createdBy, int? orderId = null, string notes = null)
        {
            try
            {
                var product = await _db.Products.FindAsync(productId);
                if (product == null) return false;

                var previousStock = product.StockQuantity;
                var newStock = previousStock + quantity; // quantity can be negative for sales

                // Prevent negative stock
                if (newStock < 0)
                {
                    throw new InvalidOperationException($"Insufficient stock. Available: {previousStock}, Requested: {Math.Abs(quantity)}");
                }

                // Update product stock
                product.StockQuantity = newStock;
                product.ModifiedDate = DateTime.Now;

                if (transactionType == "Purchase" || transactionType == "Adjustment")
                {
                    product.LastRestocked = DateTime.Now;
                }

                // Create inventory transaction record
                var transaction = new InventoryTransaction
                {
                    ProductID = productId,
                    TransactionType = transactionType,
                    Quantity = quantity,
                    PreviousStock = previousStock,
                    NewStock = newStock,
                    OrderID = orderId,
                    Notes = notes,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };

                _db.InventoryTransactions.Add(transaction);
                await _db.SaveChangesAsync();

                // Check for low stock alerts
                await CheckLowStockAlert(product);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Processes stock reduction for order items
        /// </summary>
        public async Task<bool> ProcessOrderStockReductionAsync(Order order, string createdBy)
        {
            try
            {
                foreach (var item in order.OrderItems)
                {
                    var success = await UpdateStockAsync(
                        item.ProductID,
                        -item.Quantity, // Negative for stock reduction
                        "Sale",
                        createdBy,
                        order.OrderID,
                        $"Order #{order.OrderNumber}"
                    );

                    if (!success) return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Restores stock for cancelled/returned orders
        /// </summary>
        public async Task<bool> RestoreOrderStockAsync(Order order, string createdBy)
        {
            try
            {
                foreach (var item in order.OrderItems)
                {
                    var success = await UpdateStockAsync(
                        item.ProductID,
                        item.Quantity, // Positive for stock restoration
                        "Return",
                        createdBy,
                        order.OrderID,
                        $"Return for Order #{order.OrderNumber}"
                    );

                    if (!success) return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if product stock is low and creates alert
        /// </summary>
        private async Task CheckLowStockAlert(Product product)
        {
            try
            {
                // Check if product is low stock or out of stock
                var alertType = "";
                if (product.StockQuantity <= 0)
                {
                    alertType = "OutOfStock";
                }
                else if (product.StockQuantity <= product.MinStockLevel)
                {
                    alertType = "LowStock";
                }

                if (!string.IsNullOrEmpty(alertType))
                {
                    // Check if alert already exists and is not resolved
                    var existingAlert = _db.StockAlerts
                        .FirstOrDefault(a => a.ProductID == product.ProductID && 
                                           a.AlertType == alertType && 
                                           !a.IsResolved);

                    if (existingAlert == null)
                    {
                        // Create new alert
                        var alert = new StockAlert
                        {
                            ProductID = product.ProductID,
                            AlertType = alertType,
                            CurrentStock = product.StockQuantity,
                            ThresholdLevel = product.MinStockLevel,
                            IsResolved = false,
                            CreatedDate = DateTime.Now
                        };

                        _db.StockAlerts.Add(alert);
                        await _db.SaveChangesAsync();

                        // Send email notification
                        await _emailService.SendLowStockAlertAsync(product, product.StockQuantity);
                    }
                }
                else
                {
                    // Resolve existing alerts if stock is now sufficient
                    var alertsToResolve = _db.StockAlerts
                        .Where(a => a.ProductID == product.ProductID && !a.IsResolved)
                        .ToList();

                    foreach (var alert in alertsToResolve)
                    {
                        alert.IsResolved = true;
                        alert.ResolvedDate = DateTime.Now;
                    }

                    if (alertsToResolve.Any())
                    {
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Log error but don't fail the main operation
            }
        }

        /// <summary>
        /// Gets inventory transactions for a product
        /// </summary>
        public IQueryable<InventoryTransaction> GetProductTransactions(int productId)
        {
            return _db.InventoryTransactions
                .Where(t => t.ProductID == productId)
                .OrderByDescending(t => t.CreatedDate);
        }

        /// <summary>
        /// Gets low stock products
        /// </summary>
        public IQueryable<Product> GetLowStockProducts()
        {
            return _db.Products
                .Where(p => p.IsActive && p.StockQuantity <= p.MinStockLevel)
                .OrderBy(p => p.StockQuantity);
        }

        /// <summary>
        /// Gets out of stock products
        /// </summary>
        public IQueryable<Product> GetOutOfStockProducts()
        {
            return _db.Products
                .Where(p => p.IsActive && p.StockQuantity <= 0)
                .OrderBy(p => p.ProductName);
        }

        /// <summary>
        /// Gets unresolved stock alerts
        /// </summary>
        public IQueryable<StockAlert> GetUnresolvedAlerts()
        {
            return _db.StockAlerts
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.CreatedDate);
        }

        public void Dispose()
        {
            _db?.Dispose();
            _emailService?.Dispose();
        }
    }
}