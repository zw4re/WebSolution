using Microsoft.AspNetCore.Builder;

namespace AdminAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Razor View (Login.cshtml gibi dosyalarý render edebilmek için gerekli)
            builder.Services.AddControllersWithViews();

            // HTTP istekleri için HttpClient
            builder.Services.AddHttpClient();

            // Cookie Authentication sistemi tanýmlanýyor
            builder.Services.AddAuthentication("AdminScheme")
                .AddCookie("AdminScheme", options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/login";
                });

            var app = builder.Build();

            // Statik dosyalar (CSS, JS, görseller)
            app.UseStaticFiles();

            // Middleware sýralamasý
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Controller ve Razor View'larý çalýþtýr
            app.MapControllers();

            // login ekraný açýlýþý
            app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
