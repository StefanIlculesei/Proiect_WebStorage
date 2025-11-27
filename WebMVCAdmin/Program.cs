using Microsoft.EntityFrameworkCore;
using PersistenceLayer;
using DataAccessLayer.Accessors;
using FluentValidation;
using FluentValidation.AspNetCore;
using WebMVCAdmin.Validators;
using WebMVCAdmin.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PlanViewModelValidator>();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=PPAW;Username=postgres;Password=acces123";
builder.Services.AddDbContext<WebStorageContext>(options =>
    options.UseNpgsql(connectionString));

// Register Accessors
builder.Services.AddScoped<PlanAccessor>();
builder.Services.AddScoped<SubscriptionAccessor>();
builder.Services.AddScoped<UserAccessor>();
// Add other accessors if needed by other controllers later

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
