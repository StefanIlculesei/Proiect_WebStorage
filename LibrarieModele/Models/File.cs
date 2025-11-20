using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarieModele.Models
{
    [Table("files")]
    public class File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("folder_id")]
        public int? FolderId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Column("file_size")]
        public long FileSize { get; set; }

        [Required]
        [MaxLength(1000)]
        [Column("storage_path")]
        public string StoragePath { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("mime_type")]
        public string? MimeType { get; set; }

        [Column("upload_date")]
        public DateTime? UploadDate { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [MaxLength(128)]
        [Column("checksum")]
        public string? Checksum { get; set; }

        [MaxLength(20)]
        [Column("visibility")]
        public string? Visibility { get; set; } // private|shared|public

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("FolderId")]
        public virtual Folder? Folder { get; set; }

        public virtual ICollection<FileEvent> FileEvents { get; set; } = new List<FileEvent>();
        public virtual ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
    }
}