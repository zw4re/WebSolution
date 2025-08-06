
using Hangfire;

namespace Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // MVC ve Razor view engine desteği ekleniyor
            builder.Services.AddControllersWithViews();
            // HttpClient servisi ekleniyor (API çağrıları için)
            builder.Services.AddHttpClient();
            // Cookie Authentication servisi ekleniyor
            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                });
            builder.Services.AddHangfire(config =>
            {
                config.UseRedisStorage("localhost:6379"); // ← Burada senin Redis adresin neyse onu yaz
            });

            builder.Services.AddHangfireServer();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Statik dosyaların (css, js, img) sunulabilmesi için
            app.UseStaticFiles();
            // Routing middleware'i ekleniyor
            app.UseRouting();
            // Kullanıcı kimlik doğrulama middleware'i
            app.UseAuthentication();
            // Kullanıcı yetkilendirme middleware'i
            app.UseAuthorization();

            // Varsayılan route: Uygulama açıldığında Account/Login ekranı açılır
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });

            app.Run();
        }
    }
}
