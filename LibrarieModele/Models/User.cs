using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarieModele.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("storage_used")]
        public long StorageUsed { get; set; } = 0;

        [MaxLength(50)]
        [Column("role")]
        public string Role { get; set; } = "user";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<Folder> Folders { get; set; } = new List<Folder>();
        public virtual ICollection<File> Files { get; set; } = new List<File>();
        public virtual ICollection<FileEvent> FileEvents { get; set; } = new List<FileEvent>();
        public virtual ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}