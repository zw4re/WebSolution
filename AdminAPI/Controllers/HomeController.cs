using Microsoft.AspNetCore.Mvc;
using AdminAPI.Services;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Net.Http;
using AdminAPI.Presentation;

namespace AdminAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redisService;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration, RedisService redisService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _redisService = redisService;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Dashboard()
        {
            var client = _httpClientFactory.CreateClient();
            var dbUrl = _configuration["URL:DatabaseService"];

            // 1. En güncel tarihi al
            var dateResponse = await client.GetAsync($"{dbUrl}/api/tcmb/last-date");
            if (!dateResponse.IsSuccessStatusCode)
                return Content("En son döviz kuru tarihi alınamadı.");

            var latestDateStr = await dateResponse.Content.ReadAsStringAsync();
            ViewData["LatestDate"] = DateTime.Parse(latestDateStr).ToString("dd.MM.yyyy");

            // 2. Bu tarihe ait alış/satış verilerini çek
            var rateResponse = await client.GetAsync($"{dbUrl}/api/tcmb/date/{latestDateStr}");
            if (!rateResponse.IsSuccessStatusCode)
                return Content("Döviz kuru verileri alınamadı.");

            var rateJson = await rateResponse.Content.ReadAsStringAsync();
            var rates = JsonSerializer.Deserialize<List<CurrencyDto>>(rateJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 3. Buy/Sell ayrı satırları tek satırda gruplandır
            var grouped = rates
                .GroupBy(x => x.CurrencyCode)
                .Select(g => new CurrencyRateDto
                {
                    CurrencyCode = g.Key,
                    Buy = g.FirstOrDefault(x => x.Type == "Buy")?.Value,
                    Sell = g.FirstOrDefault(x => x.Type == "Sell")?.Value
                })
                .ToList();

            // 4. Şirket sayısı
            var countResponse = await client.GetAsync($"{dbUrl}/api/companies/count");
            var countString = await countResponse.Content.ReadAsStringAsync();
            int companyCount = int.TryParse(countString, out var parsedCount) ? parsedCount : 0;

            // 5. Job bilgileri
            var kapJob = await _redisService.GetJobInfoAsync("kap-job");
            var tcmbJob = await _redisService.GetJobInfoAsync("tcmb-job");

            // 6. ViewData
            ViewData["Rates"] = grouped;
            ViewData["CompanyCount"] = companyCount;
            ViewData["KapJobLastRun"] = kapJob?.LastExecution ?? "-";
            ViewData["KapJobName"] = kapJob?.JobType ?? "-";
            ViewData["TcmbJobLastRun"] = tcmbJob?.LastExecution ?? "-";
            ViewData["TcmbJobName"] = tcmbJob?.JobType ?? "-";

            return View("Dashboard");
        }
    }
}
