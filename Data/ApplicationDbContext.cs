using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using myDotnetApiExcercise.Models;

namespace myDotnetApiExcercise.Data;

public class ApplicationDbContext : IdentityDbContext<SystemUser, SystemRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        //
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemUser>(entity =>
        {
            // Khai báo để tạo bảng trong database
            entity.Property(u => u.Id).HasColumnName("UserId"); // Cấu hình tên cột khóa chính cho bảng
            entity.Property(u => u.UserName).HasMaxLength(50);
            entity.Property(u => u.FullName).HasMaxLength(50);
            entity.Property(u => u.PasswordHash).HasMaxLength(128);
            entity.Property(u => u.isActive).HasDefaultValue(true);
        });

        // Đổi tên bảng
        modelBuilder.Entity<SystemUser>().ToTable("SystemUsers"); 
        modelBuilder.Entity<SystemRole>().ToTable("SystemRoles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");

        // Loại bỏ bảng không dùng
        modelBuilder.Ignore<IdentityUserClaim<Guid>>();
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();

        // ... các cấu hình tùy chỉnh khác (nếu có) ...
    }
}