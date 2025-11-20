using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibrarieModele.Models;

namespace NivelPersistenta
{
    public class WebStorageContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public WebStorageContext(DbContextOptions<WebStorageContext> options)
            : base(options)
        {
        }

        // Domain DbSets - using alias for File to avoid conflict with System.IO.File
        public DbSet<LibrarieModele.Models.File> Files { get; set; } = null!;
        public DbSet<Folder> Folders { get; set; } = null!;
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<FileEvent> FileEvents { get; set; } = null!;
        public DbSet<UsageRecord> UsageRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map Identity tables to AspNet* naming (already default, but explicit for clarity)
            // Note: AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, 
            // AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens are default names

            // Map domain tables to PascalCase (override [Table] attributes from models)
            modelBuilder.Entity<LibrarieModele.Models.File>().ToTable("Files");
            modelBuilder.Entity<Folder>().ToTable("Folders");
            modelBuilder.Entity<Plan>().ToTable("Plans");
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions");
            modelBuilder.Entity<Transaction>().ToTable("Transactions");
            modelBuilder.Entity<FileEvent>().ToTable("FileEvents");
            modelBuilder.Entity<UsageRecord>().ToTable("UsageRecords");

            // Configure relationships and delete behaviors
            // User relationships (Restrict to prevent cascading deletes)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Folders)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Files)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.FileEvents)
                .WithOne(fe => fe.User)
                .HasForeignKey(fe => fe.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UsageRecords)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Plan relationships
            modelBuilder.Entity<Plan>()
                .HasMany(p => p.Subscriptions)
                .WithOne(s => s.Plan)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Folder self-reference (optional parent, SetNull on delete)
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(p => p.SubFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Folder-Files relationship (optional folder, SetNull on delete)
            modelBuilder.Entity<Folder>()
                .HasMany(f => f.Files)
                .WithOne(file => file.Folder)
                .HasForeignKey(file => file.FolderId)
                .OnDelete(DeleteBehavior.SetNull);

            // File-FileEvents relationship (optional file, SetNull on delete)
            modelBuilder.Entity<LibrarieModele.Models.File>()
                .HasMany(f => f.FileEvents)
                .WithOne(fe => fe.File)
                .HasForeignKey(fe => fe.FileId)
                .OnDelete(DeleteBehavior.SetNull);

            // File-UsageRecords relationship (optional file, SetNull on delete)
            modelBuilder.Entity<LibrarieModele.Models.File>()
                .HasMany(f => f.UsageRecords)
                .WithOne(ur => ur.File)
                .HasForeignKey(ur => ur.FileId)
                .OnDelete(DeleteBehavior.SetNull);

            // Subscription-Transactions relationship (optional subscription, SetNull on delete)
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Transactions)
                .WithOne(t => t.Subscription)
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
