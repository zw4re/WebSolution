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
// -------------------- CORS (ÖNEMLÝ) --------------------

const string CorsPolicyName = "AllowAdminPanel";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicyName, p =>
        p.WithOrigins(
             "http://localhost:5147",
             "https://localhost:7024"
        // gerekiyorsa baþka origin/port ekle
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials() // cookie/token kullanýyorsan
    );
});
// Controller servisini ekleme 
builder.Services.AddControllersWithViews(); // View

var app = builder.Build();
// Middleware 
app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot veya css/js için gerekli
app.UseRouting();
app.UseCors(CorsPolicyName);
app.UseAuthorization();


// MVC Route 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
