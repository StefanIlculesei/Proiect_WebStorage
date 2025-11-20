using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarieModele.Models
{
    [Table("subscriptions")]
    public class Subscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("plan_id")]
        public int PlanId { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [MaxLength(50)]
        [Column("status")]
        public string? Status { get; set; } // active|canceled|expired|trialing

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("auto_renew")]
        public bool AutoRenew { get; set; } = true;

        [MaxLength(255)]
        [Column("external_subscription_id")]
        public string? ExternalSubscriptionId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PlanId")]
        public virtual Plan Plan { get; set; } = null!;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}