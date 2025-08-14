using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Hangfire.Storage;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.Linq;
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
            var apiBase = (_configuration["URL:DatabaseService"] ?? "").TrimEnd('/');
            var client = _httpClientFactory.CreateClient();

            // Şirket sayısı
            int companyCount = 0;
            var companyCountResponse = await client.GetAsync($"{apiBase}/companies/count");
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
                var lastExecutedJob = jobList?
                    .Where(j => j.LastExecution.HasValue)
                    .OrderByDescending(j => j.LastExecution)
                    .FirstOrDefault();
                lastJobTime = lastExecutedJob?.LastExecution;
            }

            // En güncel döviz kurları
            List<TcmbExchangeRate> rates = new();
            var latestRatesResponse = await client.GetAsync($"{apiBase}/tcmb/latest");
            if (latestRatesResponse.IsSuccessStatusCode)
            {
                var json = await latestRatesResponse.Content.ReadAsStringAsync();
                rates = JsonSerializer.Deserialize<List<TcmbExchangeRate>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            // Redis üzerinden job listesi
            var jobs = _redisService.GetRecurringJobs();

            var viewModel = new DashboardViewModel
            {
                CompanyCount = companyCount,
                LastJobTime = lastJobTime?.ToString("dd.MM.yyyy HH:mm") ?? "Henüz çalışmadı",
                ExchangeRates = rates,
                RecurringJobs = jobs
            };

            return View(viewModel);
        }
        //BÜTÜN DÖVİZ KURLARI

        [HttpGet("/dashboard/rates-by-date")]
        public async Task<IActionResult> RatesByDate([FromQuery] DateTime? date)
        {
            var apiBase = _configuration["URL:DatabaseService"]?.TrimEnd('/'); // ← appsettings'teki .../api kalır
            var client = _httpClientFactory.CreateClient();

            var d = (date ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd");
            var url = $"{apiBase}/tcmb/by-date?date={d}"; // ← ekstra /api EKLEME! (appsettings zaten /api ile bitiyor)

            List<TcmbExchangeRate> list = new();
            try
            {
                var resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    list = await resp.Content.ReadFromJsonAsync<List<TcmbExchangeRate>>() ?? new();
                }
                else
                {
                    ViewBag.RatesError = $"TCMB servisi {(int)resp.StatusCode} döndürdü.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.RatesError = "TCMB servisine ulaşılamadı: " + ex.Message;
            }

            var vm = new DashboardViewModel
            {
                RatesDate = DateTime.Parse(d),
                ExchangeRates = list.OrderBy(x => x.CurrencyCode).ThenBy(x => x.Type).ToList()
            };

            return PartialView("_RatesTable", vm);
        }


    }
}
