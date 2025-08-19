using Hangfire;
using Admin.Services;
using CommonMessaging; 
using RabbitMQ.Client;

namespace Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();

            // Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".Admin.Session";
                options.IdleTimeout = TimeSpan.FromHours(4);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Cookie Auth
            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                });

            // Hangfire (Redis)
            builder.Services.AddHangfire(cfg => cfg.UseRedisStorage("localhost:6379"));
            builder.Services.AddHangfireServer();

            builder.Services.AddSingleton<RedisService>();

            //  RabbitMQ MessagingOptions ayarlarını appsettings.jsondan çeker
            builder.Services.Configure<MessagingOptions>(builder.Configuration.GetSection("RabbitMQ"));

            //  RabbitConnectionı singleton olarak ekle
            builder.Services.AddSingleton<RabbitConnection>(sp =>
            {
                var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MessagingOptions>>().Value;
                return new RabbitConnection(opt);
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire");

            // Endpoint 
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
