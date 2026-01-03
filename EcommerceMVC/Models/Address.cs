using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        [StringLength(20)]
        [Display(Name = "Address Type")]
        public string AddressType { get; set; }

        [Required]
        [StringLength(100)]
        public string Street1 { get; set; }

        [StringLength(100)]
        public string Street2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [Display(Name = "Default Address")]
        public bool IsDefault { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [ForeignKey("CustomerID")]
        public virtual Customer Customer { get; set; }
    }
}
