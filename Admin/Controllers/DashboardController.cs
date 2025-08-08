using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Hangfire.Storage;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System;
using Entities.DbModels;
using Entities.Presentation; 
using Admin.Services; 

namespace Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redisService;

        public DashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration, RedisService redisService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _redisService = redisService;
        }

        [HttpGet("/dashboard")]
        public async Task<IActionResult> Index()
        {
            var apiBase = _configuration["URL:DatabaseService"];
            var client = _httpClientFactory.CreateClient();

            // Şirket sayısını çek
            int companyCount = 0;
            var companyCountUrl = apiBase.Replace("/api/login", "/api/companies/count");
            var companyCountResponse = await client.GetAsync(companyCountUrl);
            if (companyCountResponse.IsSuccessStatusCode)
            {
                var countString = await companyCountResponse.Content.ReadAsStringAsync();
                int.TryParse(countString, out companyCount);
            }

            // Son job zamanı
            DateTime? lastJobTime = null;
            using (var connection = JobStorage.Current.GetConnection())
            {
                var jobList = connection.GetRecurringJobs();

                if (jobList != null && jobList.Count > 0)
                {
                    var lastExecutedJob = jobList
                        .Where(j => j.LastExecution.HasValue)
                        .OrderByDescending(j => j.LastExecution)
                        .FirstOrDefault();

                    lastJobTime = lastExecutedJob?.LastExecution;
                }
            }

            // En güncel 5 döviz kuru verisi
            List<TcmbExchangeRate> rates = new();
            var latestRatesUrl = apiBase.Replace("/api/login", "/api/tcmb/latest");
            var latestRatesResponse = await client.GetAsync(latestRatesUrl);
            if (latestRatesResponse.IsSuccessStatusCode)
            {
                var json = await latestRatesResponse.Content.ReadAsStringAsync();
                rates = JsonSerializer.Deserialize<List<TcmbExchangeRate>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            // Redis üzerinden job listesi al
            var jobs = _redisService.GetRecurringJobs(); 

            // ViewModel oluştur
            var viewModel = new DashboardViewModel
            {
                CompanyCount = companyCount,
                LastJobTime = lastJobTime?.ToString("dd.MM.yyyy HH:mm") ?? "Henüz çalışmadı",
                ExchangeRates = rates,
                RecurringJobs = jobs 
            };

            return View(viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
