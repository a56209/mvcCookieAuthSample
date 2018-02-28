using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using mvcCookieAuthSample.Models;
using Microsoft.Extensions.DependencyInjection;

namespace mvcCookieAuthSample.Data
{
    public class ApplicationDbContextSeed
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationUserRole> _roleManager;

        public async Task SeedAsync(ApplicationDbContext context, IServiceProvider services)
        {
            if(!context.Roles.Any())
            {                
                _roleManager = services.GetRequiredService<RoleManager<ApplicationUserRole>>();

                var role = new ApplicationUserRole() { Name = "Administrators", NormalizedName = "Administrators" };
                var result = await _roleManager.CreateAsync(role);
                if(!result.Succeeded)
                {
                    throw new Exception("初始化默认角色失败:" + result.Errors.SelectMany(e => e.Description));
                }
            }

            if (!context.Users.Any())
            {
                _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                var defaultUser = new ApplicationUser {
                    UserName = "Administrator",
                    Email = "jessetalk@163.com",
                    NormalizedUserName = "admin",
                    SecurityStamp = "admin",
                    Avatar = "https://chocolatey.org/content/packageimages/dotnetcore-vs.2.0.3-Preview.png"
                };

                var result = await _userManager.CreateAsync(defaultUser, "123456");
                await _userManager.AddToRoleAsync(defaultUser, "Administrators");
                
                if (!result.Succeeded)
                {
                    throw new Exception("初始默认用户失败");
                }
            }
        }
    }
}
