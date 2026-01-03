using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryID { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Range(0, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Range(0, 999999.99)]
        [DataType(DataType.Currency)]
        [Display(Name = "Cost Price")]
        public decimal? CostPrice { get; set; }

        public decimal? Weight { get; set; }

        [StringLength(50)]
        public string Dimensions { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        // Inventory Management Fields
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Minimum Stock Level")]
        public int MinStockLevel { get; set; }

        [Display(Name = "Maximum Stock Level")]
        public int MaxStockLevel { get; set; }

        [Display(Name = "Reorder Point")]
        public int ReorderPoint { get; set; }

        [Display(Name = "Last Restocked")]
        public DateTime? LastRestocked { get; set; }

        // Review Summary Fields
        [Display(Name = "Average Rating")]
        public decimal AverageRating { get; set; }

        [Display(Name = "Review Count")]
        public int ReviewCount { get; set; }

        // Computed Properties
        [NotMapped]
        [Display(Name = "Is Low Stock")]
        public bool IsLowStock => StockQuantity <= MinStockLevel;

        [NotMapped]
        [Display(Name = "Is Out of Stock")]
        public bool IsOutOfStock => StockQuantity <= 0;

        [NotMapped]
        [Display(Name = "Stock Status")]
        public string StockStatus
        {
            get
            {
                if (StockQuantity <= 0) return "Out of Stock";
                if (StockQuantity <= MinStockLevel) return "Low Stock";
                return "In Stock";
            }
        }

        [NotMapped]
        [Display(Name = "Star Rating Display")]
        public string StarRatingDisplay
        {
            get
            {
                var fullStars = (int)Math.Floor(AverageRating);
                var hasHalfStar = AverageRating - fullStars >= 0.5m;
                var emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

                var stars = new string('★', fullStars);
                if (hasHalfStar) stars += "☆";
                stars += new string('☆', emptyStars);

                return stars;
            }
        }

        // Navigation property
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
    }
}
