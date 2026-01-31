using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using myDotnetApiExcercise.Models;

namespace myDotnetApiExcercise.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        // Lấy các dịch vụ cần thiết
        var roleManager = serviceProvider.GetRequiredService<RoleManager<SystemRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<SystemUser>>();

        // 1. Seed Roles (Tạo Vai trò Admin)
        string adminRoleName = "Admin";
        if (await roleManager.FindByNameAsync(adminRoleName) == null)
        {
            await roleManager.CreateAsync(new SystemRole() { Name = adminRoleName, NormalizedName = adminRoleName.ToUpper() });
        }

        // 2. Seed Admin User (Tạo Người dùng Admin)
        string adminUserName = configuration["InitialAdmin:Name"] ?? throw new ArgumentException("Default name not found");

        if (await userManager.FindByNameAsync(adminUserName) == null)
        {
            // Lấy Id mặc định từ cấu hình
            string userId = configuration["InitialAdmin:Guid"] ?? throw new ArgumentException("Default password not found");
            // Lấy password mặc định từ cấu hình
            string password = configuration["InitialAdmin:Password"] ?? throw new ArgumentException("Default password not found");
            var adminUser = new SystemUser
            {
                Id = Guid.Parse(userId),
                UserName = adminUserName,
                FullName = adminUserName,
                Gender = "M",
                isActive = true,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                DateOfBith = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, password);

            if (result.Succeeded)
            {
                // Gán vai trò Admin cho người dùng
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
            else
            {
                // Xử lý lỗi nếu tạo user thất bại
                Console.WriteLine($"Error creating admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}