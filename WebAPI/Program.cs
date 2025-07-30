using Hangfire;
using WebAPI.Helpers;
using Microsoft.EntityFrameworkCore;
//using Hangfire.Redis.StackExchange;
using Worker.Jobs;
using DatabaseService.Context;
using WebAPI.Services;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<CompanyService>();

            // Hangfire servislerini ekle
            builder.Services.AddHangfire(config => config.UseRedisStorage("localhost"));
            builder.Services.AddHangfireServer();

            var app = builder.Build();

            // Hangfire Dashboard (herkese açýk)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllUsersAuthorizationFilter() }
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
