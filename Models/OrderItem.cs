using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductID { get; set; }

        [Required]
        [Range(1, 9999)]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal Discount { get; set; }

        [Display(Name = "Tax Rate")]
        public decimal TaxRate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        // Computed property
        [NotMapped]
        public decimal LineTotal => (Quantity * UnitPrice) - Discount;
    }
}
