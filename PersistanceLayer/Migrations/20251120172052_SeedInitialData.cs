using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersistenceLayer.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Roles
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { 1, "admin", "ADMIN", Guid.NewGuid().ToString() },
                    { 2, "user", "USER", Guid.NewGuid().ToString() }
                });

            // Seed Users
            // Password for all: USV-2025
            var passwordHash = "$2a$12$hqHGZ8p9xVQ7yPwOkJ2Kce8YHX.nWZ3qVpK/F7Y8qL6zM8tO2xW2G";
            var now = DateTime.UtcNow;

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
                               "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                               "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                               "LockoutEnabled", "AccessFailedCount", "storage_used", "role", "created_at", "updated_at", "deleted_at" },
                values: new object[,] 
                { 
                    { 1, "demo", "DEMO", "demo@example.com", "DEMO@EXAMPLE.COM", true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, false, false, null, true, 0, 0L, "user", now, now, null },
                    { 2, "admin", "ADMIN", "admin@example.com", "ADMIN@EXAMPLE.COM", true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, false, false, null, true, 0, 0L, "admin", now, now, null },
                    { 3, "john_doe", "JOHN_DOE", "john@example.com", "JOHN@EXAMPLE.COM", true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, false, false, null, true, 0, 0L, "user", now, now, null },
                    { 4, "jane_smith", "JANE_SMITH", "jane@example.com", "JANE@EXAMPLE.COM", true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, false, false, null, true, 0, 0L, "user", now, now, null }
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
                });

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
                });

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
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded data
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

            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 4);

            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 2);
        }
    }
}
