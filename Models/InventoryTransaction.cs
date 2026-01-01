using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("InventoryTransactions")]
    public class InventoryTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; } // Purchase, Sale, Adjustment, Return

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Previous Stock")]
        public int PreviousStock { get; set; }

        [Required]
        [Display(Name = "New Stock")]
        public int NewStock { get; set; }

        public int? OrderID { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }
    }
}