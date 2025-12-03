using System.ComponentModel.DataAnnotations;

namespace WebMVC_Plans.Models
{
    public class PlanViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Plan Name")]
        [Required(ErrorMessage = "Plan name is required")]
        [StringLength(100, ErrorMessage = "Plan name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Storage Limit (GB)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public double LimitSizeGB { get; set; }

        [Display(Name = "Max File Size (MB)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public double MaxFileSizeMB { get; set; }

        [Display(Name = "Billing Period")]
        public string? BillingPeriod { get; set; }

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }

        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Total Subscribers")]
        public int SubscriptionCount { get; set; }

        public string FormattedPrice => $"{Price:N2} {Currency}";

        public bool IsFree => Price == 0;
    }
}
