using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarieModele.Models
{
    [Table("usage_records")]
    public class UsageRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("file_id")]
        public int? FileId { get; set; }

        [Column("event_date")]
        public DateTime? EventDate { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Column("file_size")]
        public long FileSize { get; set; }

        [MaxLength(50)]
        [Column("event_type")]
        public string? EventType { get; set; } // upload|download

        [Column("period_start", TypeName = "date")]
        public DateTime? PeriodStart { get; set; }

        [Column("period_end", TypeName = "date")]
        public DateTime? PeriodEnd { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("FileId")]
        public virtual File? File { get; set; }
    }
}