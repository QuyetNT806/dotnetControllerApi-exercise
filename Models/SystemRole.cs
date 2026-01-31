using Microsoft.AspNetCore.Identity;
namespace myDotnetApiExcercise.Models;

// Kế thừa class IdentityRole để tận dụng tính năng có sẵn của Identity mà không phải tự triển khai lại
public class SystemRole : IdentityRole<Guid> // Vì sử dụng Guid làm khóa chính nên truyền Guid vào IdentityRole<TKey>
{
    
}