using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    [Table("SystemSettings")]
    public class SystemSetting
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Setting Key")]
        public string SettingKey { get; set; }

        [Required]
        [Display(Name = "Setting Value")]
        public string SettingValue { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}