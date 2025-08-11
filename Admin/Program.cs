using Hangfire;
using Admin.Services;

namespace Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC ve Razor view engine desteği
            builder.Services.AddControllersWithViews();

            // HttpClient servisi (API çağrıları için)
            builder.Services.AddHttpClient();

            // Cookie Authentication
            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                });

            // Hangfire Redis ayarı
            builder.Services.AddHangfire(config =>
            {
                config.UseRedisStorage("localhost:6379");
            });
            builder.Services.AddHangfireServer();

            // RedisService ekleme
            builder.Services.AddSingleton<RedisService>();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Auth middleware
            app.UseAuthentication();
            app.UseAuthorization();

            // Hangfire Dashboard
            app.UseHangfireDashboard("/hangfire");

            app.UseEndpoints(endpoints =>
            {
                // API Controller'lar
                endpoints.MapControllers();

                // Varsayılan route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });

            app.Run();
        }
    }
}
