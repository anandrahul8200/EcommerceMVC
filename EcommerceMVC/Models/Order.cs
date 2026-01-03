using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerID { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        [Required]
        [Display(Name = "Shipping Address")]
        public int ShippingAddressID { get; set; }

        [Required]
        [Display(Name = "Billing Address")]
        public int BillingAddressID { get; set; }

        [StringLength(20)]
        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; }

        [StringLength(20)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Tax")]
        public decimal TaxAmount { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Shipping")]
        public decimal ShippingAmount { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Discount")]
        public decimal DiscountAmount { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Total")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
