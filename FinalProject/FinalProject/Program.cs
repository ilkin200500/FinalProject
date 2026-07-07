using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 5;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(25);
}).AddDefaultTokenProviders().AddEntityFrameworkStores<CourseDbContext>();
builder.Services.AddDbContext<CourseDbContext>(options =>
{
    options.UseSqlServer("Server=LENOVO\\SQLEXPRESS01;Database=FinalProjectDb;Trusted_Connection=True;TrustServerCertificate=True");
});
builder.Services.AddControllersWithViews();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
          );
app.MapControllerRoute(
    
    name:"default",
    pattern:"{controller=Home}/{action=Index}"
    
    );



app.Run();
