using Worker.Services;
using Worker.Jobs;
using Worker;
using Hangfire;
using Hangfire.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Uygulama Ayarlarý
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Servis Kayýtlarý
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Job ve Service baðýmlýlýklarý
builder.Services.AddScoped<KapParseService>();
builder.Services.AddScoped<KapJob>();
builder.Services.AddScoped<TcmbService>();
builder.Services.AddScoped<TcmbJob>();

// Recurring Job yöneticisi
builder.Services.AddSingleton<RecurringJobs>();

// Hosted worker 
builder.Services.AddHostedService<Workers>();

// Hangfire + Redis konfigürasyonu
builder.Services.AddHangfire(config =>
{
    var redisConn = builder.Configuration.GetConnectionString("RedisConnection");

    config
        .UseRedisStorage(redisConn)
        .UseActivator(new AspNetCoreJobActivator(builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>()));
});

builder.Services.AddHangfireServer();

var app = builder.Build();

// Routing ve Middleware
app.UseRouting();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapGet("/", () => "Worker API + Hangfire Dashboard aktif!");

// Recurring job'larý Hangfire'a kaydeder
var recurringJobs = app.Services.GetRequiredService<RecurringJobs>();
recurringJobs.AddOrUpdate();

// Uygulamayý baþlatýr
app.Run();
