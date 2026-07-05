using FinalProject.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CourseDbContext>(options =>
{
    options.UseSqlServer("Server=LENOVO\\SQLEXPRESS01;Database=FinalProjectDb;Trusted_Connection=True;TrustServerCertificate=True");
});
builder.Services.AddControllersWithViews();
var app = builder.Build();
app.UseStaticFiles();
app.MapControllerRoute(
    
    name:"default",
    pattern:"{controller=Home}/{action=Index}"
    
    );



app.Run();
