using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 1. SERVICES CONFIGURATION (XİDMƏTLƏRİN QURULMASI)
// =============================================================

// Identity konfiqurasiyası (IdentityRole<int> ilə və tam yumşaldılmış şifrə qaydaları)
builder.Services.AddIdentity<Member, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireDigit = false;            // Rəqəm məcburi olmasın
    options.Password.RequireLowercase = false;        // Kiçik hərf məcburi olmasın
    options.Password.RequireUppercase = false;        // Böyük hərf məcburi olmasın
    options.Password.RequireNonAlphanumeric = false;  // Simvol məcburi olmasın

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(25);
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<CourseDbContext>();

// DbContext bağlantısı
builder.Services.AddDbContext<CourseDbContext>(options =>
{
    options.UseSqlServer("Server=LENOVO\\SQLEXPRESS01;Database=FinalProjectDb;Trusted_Connection=True;TrustServerCertificate=True");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// =============================================================
// 2. MIDDLEWARES (ARA PROQRAM TƏMİNATLARI)
// =============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =============================================================
// 3. 🔥 ASİNXRON DATA SEEDER ÇAĞIRIŞI
// =============================================================
await SeedDataAsync(app);

// =============================================================
// 4. ROUTES (MARŞRUTLAR)
// =============================================================

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}"
);

// Tətbiqi başlat
await app.RunAsync();

// =============================================================
// 5. 🔥 ASİNXRON SEED DATA METODU (YENİ VƏ TƏHLÜKƏSİZ)
// =============================================================
async Task SeedDataAsync(WebApplication webApp)
{
    using (var scope = webApp.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var userManager = services.GetRequiredService<UserManager<Member>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

            // Rolların yaradılması
            string[] roleNames = { "Admin", "Student", "Teacher" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                }
            }

            // 🎯 Admin məlumatları
            string adminEmail = "admin@gmail.com";

            // Həm Email həm də UserName-ə görə yoxlama
            var adminUser = await userManager.FindByEmailAsync(adminEmail) ?? await userManager.FindByNameAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new Member
                {
                    UserName = adminEmail, // Həm username, həm email eynidir!
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Sistem Admini", // 👈 BAX BU SƏTRİ ƏLAVƏ ET!
                    isActivated = true,
                    isDeleted = false
                };

                // Şifrə ilə birlikdə admini yaradırıq
                var createAdmin = await userManager.CreateAsync(admin, "Admin123!");

                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine("==================================================");
                    Console.WriteLine("✅ ADMİN UĞURLA YARADILDI VƏ BAZAYA YAZILDI!");
                    Console.WriteLine("==================================================");
                }
                else
                {
                    Console.WriteLine("==================================================");
                    Console.WriteLine("❌ ADMİN YARADILA BİLMƏDİ! SƏBƏBLƏR:");
                    foreach (var error in createAdmin.Errors)
                    {
                        Console.WriteLine($"- Kod: {error.Code}, Açıqlama: {error.Description}");
                    }
                    Console.WriteLine("==================================================");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Bu admin artıq sistemdə (bazada) mövcuddur.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Baza ilkin məlumatlarla doldurularkən xəta baş verdi.");
            Console.WriteLine($"💥 CRITICAL EXCEPTION: {ex.Message}");
        }
    }
}