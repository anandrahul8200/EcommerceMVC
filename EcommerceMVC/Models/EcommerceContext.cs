using System.Data.Entity;

namespace EcommerceMVC.Models
{
    /// <summary>
    /// Entity Framework DbContext for the Ecommerce database
    /// This class manages database connections and entity mappings
    /// </summary>
    public class EcommerceContext : DbContext
    {
        // Constructor - uses connection string named "EcommerceDB" from Web.config
        public EcommerceContext() : base("name=EcommerceDB")
        {
            // Disable lazy loading for better performance
            this.Configuration.LazyLoadingEnabled = false;
            
            // Disable proxy creation
            this.Configuration.ProxyCreationEnabled = false;
        }

        // DbSet properties - each represents a table in the database
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Enhanced Features DbSets
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ReviewVote> ReviewVotes { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<EmailNotification> EmailNotifications { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }

        // Note: CustomerOrderHistory and LowStockProduct views removed to fix Entity Framework validation errors

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure decimal precision for currency fields
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TaxAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Discount)
                .HasPrecision(10, 2);

            // Configure relationships for enhanced features
            
            // Product Reviews
            modelBuilder.Entity<ProductReview>()
                .HasRequired(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductID);

            modelBuilder.Entity<ProductReview>()
                .HasRequired(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerID);

            modelBuilder.Entity<ProductReview>()
                .HasOptional(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderID);

            // Review Votes
            modelBuilder.Entity<ReviewVote>()
                .HasRequired(v => v.Review)
                .WithMany()
                .HasForeignKey(v => v.ReviewID);

            modelBuilder.Entity<ReviewVote>()
                .HasRequired(v => v.Customer)
                .WithMany()
                .HasForeignKey(v => v.CustomerID);

            // Inventory Transactions
            modelBuilder.Entity<InventoryTransaction>()
                .HasRequired(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductID);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOptional(t => t.Order)
                .WithMany()
                .HasForeignKey(t => t.OrderID);

            // Email Notifications
            modelBuilder.Entity<EmailNotification>()
                .HasOptional(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderID);

            modelBuilder.Entity<EmailNotification>()
                .HasOptional(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerID);

            // Stock Alerts
            modelBuilder.Entity<StockAlert>()
                .HasRequired(a => a.Product)
                .WithMany()
                .HasForeignKey(a => a.ProductID);

            // Configure decimal precision for new fields
            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasPrecision(3, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
