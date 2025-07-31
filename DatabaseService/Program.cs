using DatabaseService;
using DatabaseService.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Controller servisini ekleme 
builder.Services.AddControllersWithViews(); // View

var app = builder.Build();

app.UseStaticFiles(); // wwwroot veya css/js için gerekli
app.UseRouting();
app.UseAuthorization();
app.UseHttpsRedirection();

// MVC Route 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
