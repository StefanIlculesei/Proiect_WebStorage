using System.ComponentModel.DataAnnotations;

namespace WebMVC_Plans.Models
{

    public class SubscriptionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "User ID")]
        public int UserId { get; set; } 

        [Display(Name = "Plan ID")]
        public int PlanId { get; set; }

        [Display(Name = "User")]
        public string? UserName { get; set; }

        [Display(Name = "Plan")]
        public string? PlanName { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

       

      
    }


}