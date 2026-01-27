namespace ifmisIdentity.Data
{
    using ifmisIdentity.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            

            logger.LogInformation("Starting identity seeding...");

            try
            {
                // Step 1: Seed Roles
                var roles = new[] { "ADMIN", "USER", "MANAGER" };
                foreach (var roleName in roles)
                {
                    if (await roleManager.FindByNameAsync(roleName) == null)
                    {
                        var role = new Role
                        {
                            Name = roleName,
                            NormalizedName = roleName.ToUpper()
                        };

                        var roleResult = await roleManager.CreateAsync(role);
                        if (!roleResult.Succeeded)
                        {
                            throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", roleResult.Errors)}");
                        }

                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                }

                // Step 2: Seed Organizations
                var organizations = new List<Organization>
                {
                    new Organization { Name = "Org A", DatabaseName = "DB_A", Description = "Organization A", OrgUrl = "https://orga.example.com" },
                    new Organization { Name = "Org B", DatabaseName = "DB_B", Description = "Organization B", OrgUrl = "https://orgb.example.com" }
                };

                foreach (var org in organizations)
                {
                    var existingOrg = await dbContext.Set<Organization>().FindAsync(org.Id);
                    if (existingOrg == null)
                    {
                        await dbContext.Set<Organization>().AddAsync(org);
                        logger.LogInformation($"Organization '{org.Name}' created successfully.");
                    }
                }

                await dbContext.SaveChangesAsync();

                // Step 3: Seed Admin User and Assign to Organization
                const string adminRoleName = "ADMIN";
                const string adminUserName = "admin";
                const string adminEmail = "admin@example.com";
                const string adminPassword = "Admin123$";

                var adminUser = await userManager.FindByNameAsync(adminUserName);
                if (adminUser == null)
                {
                    var orgA = await dbContext.Set<Organization>().FirstOrDefaultAsync(o => o.Name == "Org A");

                    adminUser = new User
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        OrganizationId = orgA.Id // Assign admin to Org A
                    };

                    var userResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!userResult.Succeeded)
                    {
                        throw new Exception($"Failed to create admin user: {string.Join(", ", userResult.Errors)}");
                    }

                    logger.LogInformation($"Admin user '{adminUserName}' created successfully.");
                }

                if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);
                    if (!addToRoleResult.Succeeded)
                    {
                        throw new Exception($"Failed to add user to role '{adminRoleName}': {string.Join(", ", addToRoleResult.Errors)}");
                    }

                    logger.LogInformation($"User '{adminUserName}' added to role '{adminRoleName}' successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding identity data.");
                throw;
            }

            logger.LogInformation("Identity seeding completed successfully.");
        }

        internal static async Task SeedAsync(IServiceProvider services)
        {
            throw new NotImplementedException();
        }
    }
}
