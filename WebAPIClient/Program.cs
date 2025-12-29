using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersistenceLayer;
using DataAccessLayer.Accessors;
using FluentValidation;
using FluentValidation.AspNetCore;
using ModelLibrary.Models;
using System.Text;
using WebAPIClient.Mappers;
using WebAPIClient.Validators;
using Microsoft.AspNetCore.Http.Features;
using LoggingLayer;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
builder.Logging.AddSerilogLogging();

// Configure request size limits (increase from default 30MB to 5GB for file uploads)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 5368709120; // 5GB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5368709120; // 5GB
});

// Add services to the container.
builder.Services.AddControllers();
// ServiceLayer DI
builder.Services.AddMemoryCache();
builder.Services.Configure<ServiceLayer.Options.CacheOptions>(
    builder.Configuration.GetSection("CacheOptions"));
// Register data access dependencies
builder.Services.AddScoped<DataAccessLayer.Accessors.FileAccessor>();
builder.Services.AddScoped<DataAccessLayer.Accessors.FileEventAccessor>();
builder.Services.AddScoped<DataAccessLayer.Accessors.FolderAccessor>();
builder.Services.AddScoped<DataAccessLayer.Accessors.UserAccessor>();
builder.Services.AddScoped<DataAccessLayer.Accessors.PlanAccessor>();
builder.Services.AddScoped<DataAccessLayer.Accessors.SubscriptionAccessor>();
// Register file service with caching decorator
builder.Services.AddScoped<ServiceLayer.Interfaces.IFileService>(sp =>
{
    var inner = new ServiceLayer.Implementations.FileService(
        sp.GetRequiredService<DataAccessLayer.Accessors.FileAccessor>(),
        sp.GetRequiredService<DataAccessLayer.Accessors.FileEventAccessor>(),
        sp.GetRequiredService<DataAccessLayer.Accessors.UserAccessor>(),
        sp.GetRequiredService<DataAccessLayer.Accessors.FolderAccessor>(),
        sp.GetRequiredService<DataAccessLayer.Accessors.PlanAccessor>(),
        sp.GetRequiredService<DataAccessLayer.Accessors.SubscriptionAccessor>(),
        sp.GetRequiredService<PersistenceLayer.WebStorageContext>(),
        sp.GetRequiredService<ILogger<ServiceLayer.Implementations.FileService>>()
    );
    var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceLayer.Options.CacheOptions>>();
    var logger = sp.GetRequiredService<ILogger<ServiceLayer.Implementations.CachedFileService>>();
    return new ServiceLayer.Implementations.CachedFileService(inner, cache, opts, logger);
});

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Configure form options for large file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 5368709120; // 5GB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(ApiMappingProfile));

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WebStorageContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<WebStorageContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register Accessors
builder.Services.AddScoped<PlanAccessor>();
builder.Services.AddScoped<SubscriptionAccessor>();
builder.Services.AddScoped<UserAccessor>();
builder.Services.AddScoped<FileAccessor>();
builder.Services.AddScoped<FolderAccessor>();

// Configure email service for error notifications
builder.Services.AddSingleton<LoggingLayer.EmailService>();

// Configure OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize email service for error logging
using (var scope = app.Services.CreateScope())
{
    var emailService = scope.ServiceProvider.GetRequiredService<LoggingLayer.EmailService>();
    LoggingLayer.LoggerExtensions.ConfigureEmailService(emailService);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
