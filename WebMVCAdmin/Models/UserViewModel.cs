using System;
using System.ComponentModel.DataAnnotations;

namespace WebMVC_Plans.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Storage Used")]
        public string StorageUsedFormatted { get; set; } = string.Empty;
        
        public long StorageUsedBytes { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        public int SubscriptionCount { get; set; }
    }
}
