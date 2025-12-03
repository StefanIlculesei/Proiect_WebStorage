using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using ModelLibrary.Models;

#nullable disable

namespace PersistenceLayer.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data seeding is handled by DataSeeder.cs at application startup to utilize UserManager
                // Seed Roles
                migrationBuilder.InsertData(
                    table: "AspNetRoles",
                    columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                    values: new object[,]
                    {
                        { 1, "admin", "ADMIN", Guid.NewGuid().ToString() },
                        { 2, "user", "USER", Guid.NewGuid().ToString() }
                    });

                // Seed Users with Identity-compatible hashes
                var hasher = new PasswordHasher<User>();
                var now = DateTime.UtcNow;

                object[] CreateUser(int id, string username, string email, string role)
                {
                    var user = new User
                    {
                        Id = id,
                        UserName = username,
                        NormalizedUserName = username.ToUpperInvariant(),
                        Email = email,
                        NormalizedEmail = email.ToUpperInvariant(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        LockoutEnabled = true,
                        Role = role,
                        CreatedAt = now,
                        UpdatedAt = now,
                        StorageUsed = 0
                    };
                    user.PasswordHash = hasher.HashPassword(user, "USV-2025");

                    return new object[]
                    {
                        user.Id,
                        user.UserName,
                        user.NormalizedUserName,
                        user.Email,
                        user.NormalizedEmail,
                        user.EmailConfirmed,
                        user.PasswordHash,
                        user.SecurityStamp,
                        user.ConcurrencyStamp,
                        null, // PhoneNumber
                        false, // PhoneNumberConfirmed
                        false, // TwoFactorEnabled
                        null, // LockoutEnd
                        user.LockoutEnabled,
                        0, // AccessFailedCount
                        user.StorageUsed,
                        user.Role,
                        user.CreatedAt,
                        user.UpdatedAt,
                        null // DeletedAt
                    };
                }

                // Insert users individually to avoid nested array initializer constraints
                migrationBuilder.InsertData(
                    table: "AspNetUsers",
                    columns: new[]
                    {
                        "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                        "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                        "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                        "LockoutEnabled", "AccessFailedCount", "storage_used", "role",
                        "created_at", "updated_at", "deleted_at"
                    },
                    values: CreateUser(1, "demo", "demo@example.com", "user")
                );

                migrationBuilder.InsertData(
                    table: "AspNetUsers",
                    columns: new[]
                    {
                        "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                        "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                        "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                        "LockoutEnabled", "AccessFailedCount", "storage_used", "role",
                        "created_at", "updated_at", "deleted_at"
                    },
                    values: CreateUser(2, "admin", "admin@example.com", "admin")
                );

                migrationBuilder.InsertData(
                    table: "AspNetUsers",
                    columns: new[]
                    {
                        "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                        "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                        "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                        "LockoutEnabled", "AccessFailedCount", "storage_used", "role",
                        "created_at", "updated_at", "deleted_at"
                    },
                    values: CreateUser(3, "john_doe", "john@example.com", "user")
                );

                migrationBuilder.InsertData(
                    table: "AspNetUsers",
                    columns: new[]
                    {
                        "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                        "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                        "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                        "LockoutEnabled", "AccessFailedCount", "storage_used", "role",
                        "created_at", "updated_at", "deleted_at"
                    },
                    values: CreateUser(4, "jane_smith", "jane@example.com", "user")
                );

                // Seed User-Role relationships
                migrationBuilder.InsertData(
                    table: "AspNetUserRoles",
                    columns: new[] { "UserId", "RoleId" },
                    values: new object[,]
                    {
                        { 1, 2 }, // demo -> user role
                        { 2, 1 }, // admin -> admin role
                        { 3, 2 }, // john_doe -> user role
                        { 4, 2 }  // jane_smith -> user role
                    });

                // Seed Plans
                migrationBuilder.InsertData(
                    table: "Plans",
                    columns: new[] { "id", "name", "limit_size", "max_file_size", "billing_period", "price", "currency", "created_at", "updated_at" },
                    values: new object[,]
                    {
                        { 1, "Free", 5368709120L, 104857600L, "Monthly", 0m, "USD", now, now },      // 5 GB, 100MB max file
                        { 2, "Pro", 107374182400L, 5368709120L, "Monthly", 9.99m, "USD", now, now },  // 100 GB, 5GB max file
                        { 3, "Business", 1099511627776L, 10737418240L, "Monthly", 29.99m, "USD", now, now } // 1 TB, 10GB max file
                    }
                );

                // Seed Subscriptions
                migrationBuilder.InsertData(
                    table: "Subscriptions",
                    columns: new[] { "id", "user_id", "plan_id", "start_date", "end_date", "status", "is_active", "auto_renew", "external_subscription_id", "created_at", "updated_at" },
                    values: new object[,]
                    {
                        { 1, 1, 1, now, null, "Active", true, true, null, now, now }, // Demo -> Free
                        { 2, 2, 3, now, null, "Active", true, true, null, now, now }, // Admin -> Business
                        { 3, 3, 1, now, null, "Active", true, true, null, now, now }, // John -> Free
                        { 4, 4, 2, now, null, "Active", true, true, null, now, now }  // Jane -> Pro
                    }
                );

                // Seed root folders
                migrationBuilder.InsertData(
                    table: "Folders",
                    columns: new[] { "id", "user_id", "parent_folder_id", "name", "created_at", "updated_at", "is_deleted", "deleted_at" },
                    values: new object[,]
                    {
                        { 1, 1, null, "Root", now, now, false, null },
                        { 2, 2, null, "Root", now, now, false, null },
                        { 3, 3, null, "Root", now, now, false, null },
                        { 4, 4, null, "Root", now, now, false, null }
                    }
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data removal is handled by DataSeeder.cs logic or manual cleanup
                // Remove seeded data in reverse order
                migrationBuilder.DeleteData(table: "Folders", keyColumn: "id", keyValue: 1);
                migrationBuilder.DeleteData(table: "Folders", keyColumn: "id", keyValue: 2);
                migrationBuilder.DeleteData(table: "Folders", keyColumn: "id", keyValue: 3);
                migrationBuilder.DeleteData(table: "Folders", keyColumn: "id", keyValue: 4);

                migrationBuilder.DeleteData(table: "Subscriptions", keyColumn: "id", keyValue: 1);
                migrationBuilder.DeleteData(table: "Subscriptions", keyColumn: "id", keyValue: 2);
                migrationBuilder.DeleteData(table: "Subscriptions", keyColumn: "id", keyValue: 3);
                migrationBuilder.DeleteData(table: "Subscriptions", keyColumn: "id", keyValue: 4);

                migrationBuilder.DeleteData(table: "Plans", keyColumn: "id", keyValue: 1);
                migrationBuilder.DeleteData(table: "Plans", keyColumn: "id", keyValue: 2);
                migrationBuilder.DeleteData(table: "Plans", keyColumn: "id", keyValue: 3);

                // Remove user-role relationships
                migrationBuilder.DeleteData(table: "AspNetUserRoles", keyColumns: new[] { "UserId", "RoleId" }, keyValues: new object[] { 1, 2 });
                migrationBuilder.DeleteData(table: "AspNetUserRoles", keyColumns: new[] { "UserId", "RoleId" }, keyValues: new object[] { 2, 1 });
                migrationBuilder.DeleteData(table: "AspNetUserRoles", keyColumns: new[] { "UserId", "RoleId" }, keyValues: new object[] { 3, 2 });
                migrationBuilder.DeleteData(table: "AspNetUserRoles", keyColumns: new[] { "UserId", "RoleId" }, keyValues: new object[] { 4, 2 });

                migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 1);
                migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 2);
                migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 3);
                migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 4);

                migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 1);
                migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 2);
        }
    }
}
