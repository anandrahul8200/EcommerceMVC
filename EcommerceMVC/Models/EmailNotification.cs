using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("EmailNotifications")]
    public class EmailNotification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Email Type")]
        public string EmailType { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Recipient Email")]
        public string RecipientEmail { get; set; }

        [StringLength(200)]
        [Display(Name = "Recipient Name")]
        public string RecipientName { get; set; }

        [Required]
        [StringLength(500)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public int? OrderID { get; set; }
        public int? CustomerID { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Sent Date")]
        public DateTime? SentDate { get; set; }

        [Display(Name = "Error Message")]
        public string ErrorMessage { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Customer Customer { get; set; }
    }
}