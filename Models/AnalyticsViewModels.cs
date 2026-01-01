using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
    public class AnalyticsDashboardViewModel
    {
        // Sales Summary
        [Display(Name = "Today's Sales")]
        [DataType(DataType.Currency)]
        public decimal TodaySales { get; set; }

        [Display(Name = "This Month's Sales")]
        [DataType(DataType.Currency)]
        public decimal ThisMonthSales { get; set; }

        [Display(Name = "Last Month's Sales")]
        [DataType(DataType.Currency)]
        public decimal LastMonthSales { get; set; }

        // Order Summary
        [Display(Name = "Today's Orders")]
        public int TodayOrders { get; set; }

        [Display(Name = "This Month's Orders")]
        public int ThisMonthOrders { get; set; }

        [Display(Name = "Pending Orders")]
        public int PendingOrders { get; set; }

        // Customer Summary
        [Display(Name = "Total Customers")]
        public int TotalCustomers { get; set; }

        [Display(Name = "New Customers This Month")]
        public int NewCustomersThisMonth { get; set; }

        // Inventory Summary
        [Display(Name = "Low Stock Products")]
        public int LowStockCount { get; set; }

        [Display(Name = "Out of Stock Products")]
        public int OutOfStockCount { get; set; }

        [Display(Name = "Total Products")]
        public int TotalProducts { get; set; }

        // Review Summary
        [Display(Name = "Pending Reviews")]
        public int PendingReviews { get; set; }

        [Display(Name = "Total Reviews")]
        public int TotalReviews { get; set; }

        [Display(Name = "Average Rating")]
        public decimal AverageRating { get; set; }

        // Computed Properties
        public decimal SalesGrowth => LastMonthSales > 0 ? ((ThisMonthSales - LastMonthSales) / LastMonthSales) * 100 : 0;
        public string StockHealthStatus => OutOfStockCount > 0 ? "Critical" : LowStockCount > 0 ? "Warning" : "Good";
    }

    public class SalesAnalyticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Order> Orders { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySalesData> DailySales { get; set; }
        public List<TopProductData> TopProducts { get; set; }
    }

    public class InventoryAnalyticsViewModel
    {
        public List<Product> LowStockProducts { get; set; }
        public List<Product> OutOfStockProducts { get; set; }
        public List<InventoryTransaction> RecentTransactions { get; set; }
        public List<StockAlert> StockAlerts { get; set; }
        public decimal InventoryValue { get; set; }
    }

    public class CustomerAnalyticsViewModel
    {
        public List<TopCustomerData> TopCustomers { get; set; }
        public List<Customer> NewCustomers { get; set; }
        public List<CustomerGrowthData> CustomerGrowth { get; set; }
    }

    public class ReviewAnalyticsViewModel
    {
        public List<ProductReview> RecentReviews { get; set; }
        public List<ProductReview> PendingReviews { get; set; }
        public List<Product> TopRatedProducts { get; set; }
        public List<RatingDistributionData> RatingDistribution { get; set; }
    }

    // Data Transfer Objects
    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public decimal Sales { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductData
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopCustomerData
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class CustomerGrowthData
    {
        public DateTime Month { get; set; }
        public int NewCustomers { get; set; }
    }

    public class RatingDistributionData
    {
        public int Rating { get; set; }
        public int Count { get; set; }
    }

    public class EmailStatsData
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class EmailTypeData
    {
        public string EmailType { get; set; }
        public int Count { get; set; }
        public decimal SuccessRate { get; set; }
    }

    public class EmailReportViewModel
    {
        public List<EmailNotification> RecentNotifications { get; set; }
        public List<EmailNotification> FailedNotifications { get; set; }
        public List<EmailStatsData> NotificationStats { get; set; }
        public List<EmailTypeData> NotificationsByType { get; set; }
    }
}