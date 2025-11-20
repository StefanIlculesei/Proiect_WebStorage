using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace LibrarieModele.Models
{
    [Table("AspNetUsers")]
    public class User : IdentityUser<int>
    {
        [Column("storage_used")]
        public long StorageUsed { get; set; } = 0;

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