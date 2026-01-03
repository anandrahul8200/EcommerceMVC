using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("StockAlerts")]
    public class StockAlert
    {
        [Key]
        public int AlertID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Alert Type")]
        public string AlertType { get; set; } // LowStock, OutOfStock

        [Required]
        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [Required]
        [Display(Name = "Threshold Level")]
        public int ThresholdLevel { get; set; }

        [Display(Name = "Is Resolved")]
        public bool IsResolved { get; set; }

        public DateTime CreatedDate { get; set; }

        [Display(Name = "Resolved Date")]
        public DateTime? ResolvedDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}