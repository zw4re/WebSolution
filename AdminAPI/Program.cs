using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;

namespace AdminAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Razor View desteði
            builder.Services.AddControllersWithViews();

            // HttpClient servisi (API çaðrýlarý için)
            builder.Services.AddHttpClient();

            // Cookie Authentication tanýmý (Login/Logout gibi iþlemler için)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/login";
                });

            // RedisService register ediliyor (Job bilgilerini çekebilmek için)
            builder.Services.AddSingleton<Services.RedisService>();

            var app = builder.Build();

            // wwwroot gibi statik dosyalarý sunabilmek için
            app.UseStaticFiles();

            // Middleware pipeline
            app.UseRouting();

            // Kimlik doðrulama ve yetkilendirme
            app.UseAuthentication();
            app.UseAuthorization();

            // Razor view'larý yönetecek olan varsayýlan route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            // API controller route'larý da çalýþsýn
            app.MapControllers();

            // Uygulamayý baþlat
            app.Run();
        }
    }
}
