using Worker.Services;
using Worker.Jobs;
using Worker;
using Hangfire;
using Hangfire.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Worker.Filters;
using DatabaseService.Context;

var builder = WebApplication.CreateBuilder(args);

// Uygulama Ayarlarý
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Servis Kayýtlarý
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

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
app.UseAuthentication();
app.UseAuthorization();


// Hangfire dashboard ekranýný þifre ile giriþ
var options = new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
};

// Hangfire dashboard aktif edilir
app.UseHangfireDashboard("/hangfire", options);

app.MapControllers();

//hangfire yönlendiriyor
app.MapGet("/", context =>
{
    context.Response.Redirect("/login");
    return Task.CompletedTask;
});

// Recurring job'larý Hangfire'a kaydeder
var recurringJobs = app.Services.GetRequiredService<RecurringJobs>();
recurringJobs.AddOrUpdate();

// Uygulamayý baþlatýr
app.Run();
