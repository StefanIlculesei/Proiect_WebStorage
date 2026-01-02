using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelLibrary.Models
{
    [Table("plans")]
    public class Plan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("limit_size")]
        public long LimitSize { get; set; } = 0;

        [Column("max_file_size")]
        public long MaxFileSize { get; set; } = 0;

        [MaxLength(20)]
        [Column("billing_period")]
        public string? BillingPeriod { get; set; } // monthly|yearly|one_time

        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } = 0;

        [Required]
        [MaxLength(3)]
        [Column("currency")]
        public string Currency { get; set; } = "USD";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}