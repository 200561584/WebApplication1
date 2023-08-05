using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApplication1 {

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("AppDBContextConnection") ?? throw new InvalidOperationException("Connection string 'AppDBContextConnection' not found.");

            builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                 .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDBContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();
            //builder.Services.AddScoped<IAuthorizationMiddlewareService, AuthorizationMiddlewareService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Define your roles here
                string[] roles = { "Admin", "Manager", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                              
                 // Define the roles the users should be assigned to
                // Define your users and their corresponding roles here
                var usersWithRoles = new Dictionary<string, string>
                {
                    { "user1@example.com", "Admin" },     // Assign user1 to Admin role
                    { "user2@example.com", "Manager" }, // Assign user2 to Moderator role
                    { "user3@example.com", "User" }, // Assign user2 to Moderator role
                };
                foreach (var userRole in usersWithRoles)
                {
                    var email = userRole.Key;
                    var role = userRole.Value;

                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
                        await userManager.CreateAsync(user, "YourPassword123!"); // Set the desired password

                        // Assign the role to the user
                        if (await roleManager.RoleExistsAsync(role))
                        {
                            await userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
                    
                }

          
            app.Run();

        }
    }
}