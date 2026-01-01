using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string TransactionID { get; set; }

        [StringLength(20)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }
    }
}
