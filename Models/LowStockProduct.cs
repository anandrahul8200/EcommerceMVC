using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
    public class LowStockProduct
    {
        [Key]
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string CategoryName { get; set; }
        public string WarehouseLocation { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public DateTime? LastRestockDate { get; set; }
    }
}
