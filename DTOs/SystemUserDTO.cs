namespace myDotnetApiExcercise.DTOs;

public class SystemUserDto
{
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public required string Password { get; set; }
    public bool isActive { get; set; }
    public DateTime DateOfBith { get; set; }
    public required string Gender { get; set; }
}

public class SystemUserInfoDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public required string FullName { get; set; }
    public bool isActive { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string Gender { get; set; }
    public string email { get; set; } = "";
    public string phoneNumber { get; set; } = "";
}

public class LoginModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}