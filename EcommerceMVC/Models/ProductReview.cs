using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("ProductReviews")]
    public class ProductReview
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        public int? OrderID { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Review Title")]
        public string ReviewTitle { get; set; }

        [Display(Name = "Review")]
        public string ReviewText { get; set; }

        [Display(Name = "Verified Purchase")]
        public bool IsVerifiedPurchase { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Helpful Votes")]
        public int HelpfulVotes { get; set; }

        [Display(Name = "Unhelpful Votes")]
        public int UnhelpfulVotes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Order Order { get; set; }
    }
}