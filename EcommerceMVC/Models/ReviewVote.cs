using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("ReviewVotes")]
    public class ReviewVote
    {
        [Key]
        public int VoteID { get; set; }

        [Required]
        public int ReviewID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        [Required]
        public bool IsHelpful { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ProductReview Review { get; set; }
        public virtual Customer Customer { get; set; }
    }
}