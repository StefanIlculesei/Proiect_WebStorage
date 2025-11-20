using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarieModele.Models
{
    [Table("file_events")]
    public class FileEvent
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

        [MaxLength(50)]
        [Column("action")]
        public string? Action { get; set; } // upload|delete|download|share|move

        [Column("event_date")]
        public DateTime? EventDate { get; set; }

        [Column("file_size")]
        public long? FileSize { get; set; }

        [Column("meta", TypeName = "text")]
        public string? Meta { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("FileId")]
        public virtual File? File { get; set; }
    }
}