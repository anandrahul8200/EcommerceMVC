using System;

namespace EcommerceMVC.Models
{
    [Serializable]
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        
        public decimal Subtotal => Price * Quantity;
    }
}
