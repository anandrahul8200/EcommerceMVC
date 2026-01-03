using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
    public class CheckoutOptionViewModel
    {
        public string CheckoutType { get; set; } // "login", "register", "guest"
        
        // Login fields
        [Display(Name = "Username")]
        public string LoginUsername { get; set; }
        
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string LoginPassword { get; set; }
        
        // Registration fields
        [Display(Name = "Username")]
        public string RegisterUsername { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress]
        public string RegisterEmail { get; set; }
        
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string RegisterPassword { get; set; }
        
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("RegisterPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "First Name")]
        public string RegisterFirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string RegisterLastName { get; set; }
    }
}