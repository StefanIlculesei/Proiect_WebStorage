using System.ComponentModel.DataAnnotations;

namespace WebMVC_Plans.Models
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Username")]
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Role")]
        [Required]
        public string Role { get; set; } = string.Empty;
        
        [Display(Name = "Storage Used (Bytes)")]
        public long StorageUsed { get; set; }
    }
}
