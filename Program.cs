using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using myDotnetApiExcercise.Infrastructure;
using myDotnetApiExcercise.Data;
using myDotnetApiExcercise.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Tự động thhêm các Controller vào web
builder.Services.AddControllers();
// Tìm hiểu thêm về cấu hình OpenAPI tại https://aka.ms/aspnet/openapi
// Thêm OpenApi vào web
builder.Services.AddOpenApi();
// Thêm giao diện swagger vào project
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 1. Cấu hình định nghĩa bảo mật (Security Definition)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Nhập 'Bearer' [space] và token của bạn vào trường bên dưới. 
                       Ví dụ: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVC...' ",
        Name = "Authorization", // Tên header
        In = ParameterLocation.Header, // Vị trí gửi
        Type = SecuritySchemeType.ApiKey, // Loại bảo mật (ApiKey)
        Scheme = "Bearer" // Loại Scheme (Bearer)
    });

    // 2. Cấu hình yêu cầu bảo mật (Security Requirement)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// Thêm DbContext từ appsettings.json
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider");
// var mariaDbVersion = builder.Configuration.GetValue<string>("MariaDbVersion");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "SqlServer")
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")).AddInterceptors(new AuditSaveChangesInterceptor());
    }
    else if (databaseProvider == "MariaDb")
    {
        options.UseMySql(
            builder.Configuration.GetConnectionString("MariaDbConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MariaDbConnection"))
            ).AddInterceptors(new AuditSaveChangesInterceptor());

        // if (Version.TryParse(mariaDbVersion, out var version))
        // {
        //     options.UseMySql(
        //     builder.Configuration.GetConnectionString("MariaDbConnection"),
        //     //ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MariaDbConnection"))
        //     new MySqlServerVersion(new Version(mariaDbVersion)) // Kiểm tra lại phiên bản MariaDB
        //     ).AddInterceptors(new AuditSaveChangesInterceptor());
        // }
        // else
        // {
        //     // Xử lý khi chuỗi version trong json bị viết sai định dạng
        //     throw new Exception("Định dạng DatabaseVersion không hợp lệ. Ví dụ đúng: 10.4.32");
        // }
    }
    else
    {
        throw new Exception($"Unsupported database provider: {databaseProvider}");
    }
});

// Cấu hình Identity để sử dụng SystemUser và SystemRole
builder.Services.AddIdentity<SystemUser, SystemRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Thêm Authentication và Jwt
var jwtKey = builder.Configuration["JwtSetting:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("'Jwt:Key' configuration is missing. Please add to appsettings.");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSetting:Issuer"],
        ValidAudience = builder.Configuration["JwtSetting:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Lấy IConfiguration từ services
        var configuration = services.GetRequiredService<IConfiguration>();

        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // Truyền IConfiguration vào phương thức Seeder
        await IdentitySeeder.SeedRolesAndAdminAsync(services, configuration);
    }
    catch (Exception ex)
    {
        // ... xử lý lỗi ...
         // Ghi lại lỗi cực kỳ chi tiết, bao gồm cả stack trace
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogCritical(ex, "LỖI CHỦ YẾU: Đã xảy ra lỗi trong quá trình Migration hoặc Seeding database. Ứng dụng sẽ dừng.");
        
        // Ném lại ngoại lệ. Đây là quan trọng nhất.
        // Việc này đảm bảo ứng dụng không tiếp tục chạy với DB chưa hoàn chỉnh.
        throw; 
    }
}

// Cấu hình HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
