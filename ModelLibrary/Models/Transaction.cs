using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelLibrary.Models
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("subscription_id")]
        public int? SubscriptionId { get; set; }

        [MaxLength(64)]
        [Column("invoice_number")]
        public string? InvoiceNumber { get; set; } // human-facing number, optional

        [MaxLength(20)]
        [Column("kind")]
        public string? Kind { get; set; } // invoice|payment

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("currency")]
        public string Currency { get; set; } = "USD";

        [Column("tax_amount", TypeName = "decimal(10,2)")]
        public decimal? TaxAmount { get; set; }

        [Column("total_amount", TypeName = "decimal(10,2)")]
        public decimal? TotalAmount { get; set; }

        [MaxLength(50)]
        [Column("status")]
        public string? Status { get; set; } // draft|issued|partially_paid|paid|pending|failed|refunded

        [Column("issued_at")]
        public DateTime? IssuedAt { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [MaxLength(255)]
        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [MaxLength(50)]
        [Column("payment_provider")]
        public string? PaymentProvider { get; set; }

        [Column("metadata", TypeName = "text")]
        public string? Metadata { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }
    }
}