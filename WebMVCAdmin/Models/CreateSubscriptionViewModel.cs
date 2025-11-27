using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebMVCAdmin.Binders;

namespace WebMVC_Plans.Models
{
    public class CreateSubscriptionViewModel
    {
        [Display(Name = "User")]
        [Required]
        public int UserId { get; set; }

        [Display(Name = "Plan")]
        [Required]
        public int PlanId { get; set; }

        [Display(Name = "Start Date")]
        [ModelBinder(BinderType = typeof(IsoDateTimeModelBinder))]
        public DateTime? StartDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "End Date")]
        [ModelBinder(BinderType = typeof(IsoDateTimeModelBinder))]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Auto Renew")]
        public bool AutoRenew { get; set; } = true;

        public IEnumerable<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Plans { get; set; } = new List<SelectListItem>();
    }
}
