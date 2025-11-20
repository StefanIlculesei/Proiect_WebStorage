using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NivelPersistenta.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: Password hash for "USV-2025" generated using PasswordHasher<User>
            var demoPasswordHash = "AQAAAAIAAYagAAAAEKxQz8VxF3N4N5Yb7wJ3mL8pQ2rH9sT6vC1dK4fM0nB5jE8gA3iL7oP2qR6sU9vW0xY=";

            // Seed Roles
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { 1, "admin", "ADMIN", Guid.NewGuid().ToString() },
                    { 2, "user", "USER", Guid.NewGuid().ToString() }
                });

            // Seed Plans
            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "id", "name", "limit_size", "max_file_size", "billing_period", "price", "currency", "created_at", "updated_at" },
                values: new object[,]
                {
                    { 1, "Free", 5000000000L, 100000000L, null, 0.00m, "USD", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "Pro", 100000000000L, 5000000000L, "monthly", 9.99m, "USD", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), null }
                });

            // Seed Demo User
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", 
                                "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", 
                                "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", 
                                "storage_used", "role", "created_at", "updated_at", "deleted_at" },
                values: new object[]
                {
                    1, "demo", "DEMO", "demo@example.com", "DEMO@EXAMPLE.COM", true,
                    demoPasswordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 
                    null, false, false, null, true, 0,
                    0L, "user", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, null
                });

            // Assign demo user to "user" role
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { 1, 2 });

            // Seed Subscription for demo user
            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "id", "user_id", "plan_id", "start_date", "end_date", "status", "is_active", "auto_renew", "external_subscription_id", "created_at", "updated_at" },
                values: new object[]
                {
                    1, 1, 2, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, "active", true, true, null, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), null
                });

            // Seed Root Folder for demo user
            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "id", "user_id", "parent_folder_id", "name", "created_at", "updated_at", "is_deleted", "deleted_at" },
                values: new object[]
                {
                    1, 1, null, "root", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, false, null
                });

            // Advance sequences to prevent conflicts with explicit IDs
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"AspNetRoles\"', 'Id'), (SELECT COALESCE(MAX(\"Id\"), 1) FROM \"AspNetRoles\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Plans\"', 'id'), (SELECT COALESCE(MAX(id), 1) FROM \"Plans\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"AspNetUsers\"', 'Id'), (SELECT COALESCE(MAX(\"Id\"), 1) FROM \"AspNetUsers\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Subscriptions\"', 'id'), (SELECT COALESCE(MAX(id), 1) FROM \"Subscriptions\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Folders\"', 'id'), (SELECT COALESCE(MAX(id), 1) FROM \"Folders\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data in reverse order (respecting foreign keys)
            migrationBuilder.DeleteData(table: "Folders", keyColumn: "id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Subscriptions", keyColumn: "id", keyValue: 1);
            migrationBuilder.DeleteData(table: "AspNetUserRoles", keyColumns: new[] { "UserId", "RoleId" }, keyValues: new object[] { 1, 2 });
            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "id", keyValue: 2);
            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValue: 2);

            // Reset sequences (optional)
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"AspNetRoles\"', 'Id'), 1, false);");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Plans\"', 'id'), 1, false);");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"AspNetUsers\"', 'Id'), 1, false);");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Subscriptions\"', 'id'), 1, false);");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Folders\"', 'id'), 1, false);");
        }
    }
}
