using MaterialManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MaterialManagement.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Seed Roles
                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Seed Admin User
                var adminEmail = "admin@materialmanagement.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var admin = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(admin, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }

                // Seed Categories
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Electronics", Description = "Electronic components and devices" },
                        new Category { Name = "Hardware", Description = "Hardware materials and tools" },
                        new Category { Name = "Office Supplies", Description = "Office and stationery items" },
                        new Category { Name = "Raw Materials", Description = "Raw materials for production" },
                        new Category { Name = "Safety Equipment", Description = "Safety gear and equipment" }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Sample Materials
                if (!context.Materials.Any())
                {
                    var electronics = context.Categories.First(c => c.Name == "Electronics");
                    var hardware = context.Categories.First(c => c.Name == "Hardware");
                    var office = context.Categories.First(c => c.Name == "Office Supplies");

                    context.Materials.AddRange(
                        new Material
                        {
                            Name = "Arduino Uno",
                            Description = "Microcontroller board based on ATmega328P",
                            SKU = "ELEC-ARD-001",
                            CategoryId = electronics.Id,
                            Quantity = 50,
                            MinimumQuantity = 10,
                            UnitPrice = 25.99m,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now
                        },
                        new Material
                        {
                            Name = "Screwdriver Set",
                            Description = "Professional 12-piece screwdriver set",
                            SKU = "HARD-SCR-001",
                            CategoryId = hardware.Id,
                            Quantity = 5,
                            MinimumQuantity = 8,
                            UnitPrice = 29.99m,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now
                        },
                        new Material
                        {
                            Name = "A4 Paper Ream",
                            Description = "500 sheets of white A4 paper",
                            SKU = "OFF-PAP-001",
                            CategoryId = office.Id,
                            Quantity = 100,
                            MinimumQuantity = 20,
                            UnitPrice = 4.99m,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
