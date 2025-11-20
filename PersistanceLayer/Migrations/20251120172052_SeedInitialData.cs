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

            // Seed Demo User (password: USV-2025)
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
                               "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                               "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                               "LockoutEnabled", "AccessFailedCount", "StorageUsed", "Role", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] 
                { 
                    1, "demo", "DEMO", "demo@example.com", "DEMO@EXAMPLE.COM",
                    true, "$2a$12$hqHGZ8p9xVQ7yPwOkJ2Kce8YHX.nWZ3qVpK/F7Y8qL6zM8tO2xW2G", // Password: USV-2025
                    Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                    null, false, false, null,
                    true, 0, 0L, "user", DateTime.UtcNow, DateTime.UtcNow, null
                });

            // Seed Plans
            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "Name", "StorageLimit", "Price", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Free", 5368709120L, 0m, true, DateTime.UtcNow, DateTime.UtcNow },      // 5 GB
                    { 2, "Pro", 107374182400L, 9.99m, true, DateTime.UtcNow, DateTime.UtcNow }  // 100 GB
                });

            // Seed Subscription for demo user
            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "UserId", "PlanId", "StartDate", "EndDate", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[] 
                { 
                    1, 1, 1, DateTime.UtcNow, null, true, DateTime.UtcNow, DateTime.UtcNow
                });

            // Seed root folder for demo user
            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "Id", "UserId", "ParentFolderId", "Name", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] 
                { 
                    1, 1, null, "Root", DateTime.UtcNow, DateTime.UtcNow, null
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded data in reverse order
            migrationBuilder.DeleteData(table: "Folders", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Subscriptions", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 2);
        }
    }
}
