using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PersistenceLayer
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly WebStorageContext _context;

        public DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, WebStorageContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            // 1. Seed Roles
            await SeedRolesAsync();

            // 2. Seed Users
            await SeedUsersAsync();

            // 3. Seed Plans
            await SeedPlansAsync();

            // 4. Seed Subscriptions & Folders
            await SeedSubscriptionsAndFoldersAsync();
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = { "admin", "user" };
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new[]
            {
                new { UserName = "demo", Email = "demo@example.com", Role = "user" },
                new { UserName = "admin", Email = "admin@example.com", Role = "admin" },
                new { UserName = "john_doe", Email = "john@example.com", Role = "user" },
                new { UserName = "jane_smith", Email = "jane@example.com", Role = "user" }
            };

            foreach (var userData in users)
            {
                if (await _userManager.FindByEmailAsync(userData.Email) == null)
                {
                    var user = new User
                    {
                        UserName = userData.UserName,
                        Email = userData.Email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        StorageUsed = 0
                    };

                    // UserManager handles password hashing automatically here
                    var result = await _userManager.CreateAsync(user, "USV-2025");
                    
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, userData.Role);
                    }
                }
            }
        }

        private async Task SeedPlansAsync()
        {
            if (!await _context.Plans.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var plans = new[]
                {
                    new Plan { Name = "Free", LimitSize = 5L * 1024 * 1024 * 1024, MaxFileSize = 100L * 1024 * 1024, BillingPeriod = "Monthly", Price = 0m, Currency = "USD", CreatedAt = now, UpdatedAt = now },
                    new Plan { Name = "Pro", LimitSize = 100L * 1024 * 1024 * 1024, MaxFileSize = 5L * 1024 * 1024 * 1024, BillingPeriod = "Monthly", Price = 9.99m, Currency = "USD", CreatedAt = now, UpdatedAt = now },
                    new Plan { Name = "Business", LimitSize = 1024L * 1024 * 1024 * 1024, MaxFileSize = 10L * 1024 * 1024 * 1024, BillingPeriod = "Monthly", Price = 29.99m, Currency = "USD", CreatedAt = now, UpdatedAt = now }
                };

                await _context.Plans.AddRangeAsync(plans);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedSubscriptionsAndFoldersAsync()
        {
            // Ensure we have plans
            var freePlan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == "Free");
            var proPlan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == "Pro");
            var businessPlan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == "Business");

            if (freePlan == null || proPlan == null || businessPlan == null) return;

            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                // Seed Subscription if not exists
                if (!await _context.Subscriptions.AnyAsync(s => s.UserId == user.Id))
                {
                    Plan planToAssign = user.UserName == "admin" ? businessPlan : 
                                        user.UserName == "jane_smith" ? proPlan : freePlan;

                    var subscription = new Subscription
                    {
                        UserId = user.Id,
                        PlanId = planToAssign.Id,
                        StartDate = DateTime.UtcNow,
                        Status = "Active",
                        IsActive = true,
                        AutoRenew = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.Subscriptions.AddAsync(subscription);
                }

                // Seed Root Folder if not exists
                if (!await _context.Folders.AnyAsync(f => f.UserId == user.Id && f.ParentFolderId == null))
                {
                    var rootFolder = new Folder
                    {
                        UserId = user.Id,
                        Name = "Root",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _context.Folders.AddAsync(rootFolder);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
