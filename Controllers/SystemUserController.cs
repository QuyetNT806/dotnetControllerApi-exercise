using Microsoft.AspNetCore.Identity;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myDotnetApiExcercise.Data;
using myDotnetApiExcercise.Models;
using myDotnetApiExcercise.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace myDotnetApiExcercise.Controllers;

[ApiController]
[Route("[controller]")]
public class SystemUserController : ControllerBase
{
    // Declare ApplicationDbContext, ILogger
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SystemUserController> _logger;
    private readonly UserManager<SystemUser> _userManager;
    public SystemUserController(
        ApplicationDbContext context,
        ILogger<SystemUserController> logger,
        UserManager<SystemUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    // Endpoint GET list items
    // GET: /SystemUser
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SystemUserInfoDto>>> GetSystemUsers()
    {
        // Ghi log khi bắt đầu gọi API
        _logger.LogInformation("Nhận yêu cầu GET danh sách SystemUsers");
        // Lấy danh sách người dùng từ database
        var systemUsers = await _context.Users.Select(u => new SystemUserInfoDto
        {
            Id = u.Id,
            UserName = u.UserName == null ? "" : u.UserName,
            FullName = u.FullName,
            isActive = u.isActive,
            DateOfBirth = u.DateOfBith,
            Gender = u.Gender,
            email = u.Email == null ? "" : u.Email,
            phoneNumber = u.PhoneNumber == null ? "" : u.PhoneNumber
        }).ToListAsync();
        _logger.LogInformation($"Trả về {systemUsers.Count} users");
        return systemUsers;
    }

    // Endpoint GET one item
    // GET: /SystemUser/00000000-0000-0000-0000-000000000000
    [HttpGet("{objectGuid}")]
    [Authorize]
    public async Task<ActionResult<SystemUser>> GetSystemUser(Guid objectGuid)
    {
        // Ghi log khi bắt đầu gọi API
        _logger.LogInformation($"Nhận yêu cầu GET một SystemUser {objectGuid}");
        // Tạo đối tượng bằng cách dùng FindAsync() tìm kiếm theo Guid
        var systemUser = await _context.Users.FindAsync(objectGuid);
        // Kiểm tra đối tượng null
        if (systemUser == null)
        {
            _logger.LogWarning("Đối tượng systemUser là null. Không tìm thấy dữ liệu");
            return NotFound();
        }
        return systemUser;
    }

    // Endpoint POST insert one item
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SystemUser>> InsertSystemUser(SystemUserDto systemUser)
    {
        // Ghi log khi bắt đầu gọi API
        _logger.LogInformation($"Nhận yêu cầu POST thêm mới systemUser");
        // Kiểm tra tính hợp lệ của model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Tạo đối tượng SystemUser mới
        var newUser = new SystemUser
        {
            UserName = systemUser.UserName,
            FullName = systemUser.FullName,
            isActive = systemUser.isActive,
            DateOfBith = systemUser.DateOfBith,
            Gender = systemUser.Gender
        };

        // Sử dụng UserManager để tạo người dùng
        var result = await _userManager.CreateAsync(newUser, systemUser.Password);

        if (result.Succeeded)
        {
            // Trả về kết quả thành công
            return CreatedAtAction("GetSystemUser", new { id = newUser.Id }, newUser);
        }
        else
        {
            // Trả về lỗi nếu có
            _logger.LogWarning("Có lỗi khi thêm mới người dùng");
            return BadRequest(result.Errors);
        }
    }

    // Endpoint PUT update one item
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> PutSystemUser(Guid objectGuid, SystemUser systemUser)
    {
        // Kiểm tra objectGuid ở URL có khớp objectGuid item
        if (objectGuid != systemUser.Id)
        {
            _logger.LogWarning("ObjectGuid trong URL không khớp với ObjectGuid của đối tượng");
            return BadRequest();
        }
        // Đánh dấu đối tượng đã thay đổi
        _context.Entry(systemUser).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Xử lý khi không tìm thấy đối tượng nào trong database
            if (!SystemUserExists(objectGuid))
            {
                _logger.LogWarning($"Bản ghi {objectGuid} Không tồn tại trong database");
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return NoContent();
    }
    private bool SystemUserExists(Guid objectGuid)
    {
        return (_context.Users?.Any(e => e.Id == objectGuid)).GetValueOrDefault();
    }

    [HttpDelete("{objectGuid}")]
    [Authorize]
    public async Task<IActionResult> DeleteSystemUser(Guid objectGuid)
    {
        // Tìm đối tượng cần xóa
        var systemUser = await _context.Users.FindAsync(objectGuid);
        if (systemUser == null)
        {
            _logger.LogWarning($"Bản ghi {objectGuid} Không tồn tại trong database");
            return NotFound();
        }
        // Xóa đối tượng trong DbSet
        _context.Users.Remove(systemUser);
        // Lưu vào database
        await _context.SaveChangesAsync();
        return NoContent();
    }
}