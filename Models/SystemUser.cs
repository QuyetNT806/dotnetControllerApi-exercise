using Microsoft.AspNetCore.Identity;
namespace myDotnetApiExcercise.Models;

// Kế thừa class IdentityUser để tận dụng tính năng có sẵn của Identity mà không phải tự triển khai lại
public class SystemUser : IdentityUser<Guid> // Vì sử dụng Guid làm khóa chính nên truyền Guid vào IdentityUser<TKey>
{
    public required string FullName { get; set; }
    public bool isActive { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime DateOfBith { get; set; }
    public string Gender { get; set; } = "M";
    public DateTime UpdateAt { get; set; }
}