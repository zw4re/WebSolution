using Entities;
using Worker.Services;
using Worker.Jobs;
using Worker;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Hangfire.Server;
using Hangfire.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Uygulama ayarlarý
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Servisler
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<KapParseService>();
builder.Services.AddScoped<KapJob>();
builder.Services.AddSingleton<RecurringJobs>();
builder.Services.AddHostedService<Workers>();

// Hangfire Redis ayarlarý (Activator HARÝÇ!)
builder.Services.AddHangfire(config =>
{
    var redisConn = builder.Configuration.GetConnectionString("RedisConnection");
    config.UseRedisStorage(redisConn);
});
builder.Services.AddHangfireServer();

// app nesnesini 
var app = builder.Build();

// Scoped servisler için activator 
GlobalConfiguration.Configuration
    .UseActivator(new AspNetCoreJobActivator(app.Services.GetRequiredService<IServiceScopeFactory>()));

// Routing ve pipeline
app.UseRouting();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapGet("/", () => "Worker API + Hangfire Dashboard aktif!");

// Recurring Job'larý ekle
var recurringJobs = app.Services.GetRequiredService<RecurringJobs>();
recurringJobs.AddOrUpdate();

// Baþlat
app.Run();
