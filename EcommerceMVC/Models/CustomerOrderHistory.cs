using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
    public class CustomerOrderHistory
    {
        [Key]
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string TrackingNumber { get; set; }
        public string ShipmentStatus { get; set; }
    }
}
