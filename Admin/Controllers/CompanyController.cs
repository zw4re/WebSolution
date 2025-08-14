using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Admin.Models;
using Entities.DbModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;   // Session için
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;

        public CompanyController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
        }

        // DatabaseService’e bağlanan, (varsa) Bearer token ekleyen HttpClient
        private HttpClient CreateDbClient()
        {
            var baseUrl = (_cfg["URL:DatabaseService"] ?? "").TrimEnd('/');
            var client = _http.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            var token = HttpContext.Session.GetString("DB_TOKEN");
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // POST: /companies/create  (Yeni şirket ekle)
        [HttpPost("/companies/create")]
        public async Task<IActionResult> Create([FromForm] CreateCompanyForm? form)
        {
            if (form is null)
                return BadRequest(new { message = "Form verisi alınamadı." });

            if (string.IsNullOrWhiteSpace(form.Name) ||
                string.IsNullOrWhiteSpace(form.StockCode) ||
                string.IsNullOrWhiteSpace(form.City) ||
                string.IsNullOrWhiteSpace(form.Type))
                return BadRequest(new { message = "Eksik alanlar var." });

            var mkkMemberOid = _cfg["CompanyDefaults:MkkMemberOid"] ?? "MKKo-PLACEHOLDER";
            var relatedMemberOid = _cfg["CompanyDefaults:RelatedMemberOid"] ?? "REL-PLACEHOLDER";
            var relatedMemberTitle = _cfg["CompanyDefaults:RelatedMemberTitle"] ?? form.Name;

            var dto = new Company
            {
                StockCode = form.StockCode,
                KapMemberTitle = form.Name,
                CityName = form.City,
                KapMemberType = form.Type,
                MkkMemberOid = mkkMemberOid,
                RelatedMemberOid = relatedMemberOid,
                RelatedMemberTitle = relatedMemberTitle
            };

            var client = CreateDbClient();
            var relativeUrl = "/api/companies";
            Console.WriteLine($"→ POST {client.BaseAddress}{relativeUrl}");

            var resp = await client.PostAsJsonAsync(relativeUrl, dto);
            var body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"← {resp.StatusCode} {resp.RequestMessage?.RequestUri}");
            Console.WriteLine($"← Body: {body}");

            if (resp.IsSuccessStatusCode) return Ok(new { message = "created" });
            if (resp.StatusCode == HttpStatusCode.Conflict) return Conflict(body);
            return StatusCode((int)resp.StatusCode, body);
        }

        // PUT: /companies/{stockCode}  (Düzenle – route: eski kod, body: yeni veriler)
        [HttpPut("/companies/{stockCode}")]
        public async Task<IActionResult> Update(string stockCode, [FromForm] CreateCompanyForm? form)
        {
            if (string.IsNullOrWhiteSpace(stockCode))
                return BadRequest(new { message = "Geçersiz stok kodu." });

            if (form is null)
                return BadRequest(new { message = "Form verisi alınamadı." });

            if (string.IsNullOrWhiteSpace(form.Name) ||
                string.IsNullOrWhiteSpace(form.StockCode) ||
                string.IsNullOrWhiteSpace(form.City) ||
                string.IsNullOrWhiteSpace(form.Type))
                return BadRequest(new { message = "Eksik alanlar var." });

            var mkkMemberOid = _cfg["CompanyDefaults:MkkMemberOid"] ?? "MKKo-PLACEHOLDER";
            var relatedMemberOid = _cfg["CompanyDefaults:RelatedMemberOid"] ?? "REL-PLACEHOLDER";
            var relatedMemberTitle = _cfg["CompanyDefaults:RelatedMemberTitle"] ?? form.Name;

            var dto = new Company
            {
                StockCode = form.StockCode, // yeni kod olabilir
                KapMemberTitle = form.Name,
                CityName = form.City,
                KapMemberType = form.Type,
                MkkMemberOid = mkkMemberOid,
                RelatedMemberOid = relatedMemberOid,
                RelatedMemberTitle = relatedMemberTitle
            };

            var client = CreateDbClient();
            var url = $"/api/companies/{Uri.EscapeDataString(stockCode)}"; // ESKİ kod path’te
            Console.WriteLine($"→ PUT {client.BaseAddress}{url}");
            var resp = await client.PutAsJsonAsync(url, dto);
            var body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"← {resp.StatusCode} {resp.RequestMessage?.RequestUri}");
            Console.WriteLine($"← Body: {body}");

            if (resp.IsSuccessStatusCode) return Ok(new { message = "updated" });
            if (resp.StatusCode == HttpStatusCode.NotFound) return NotFound(body);
            if (resp.StatusCode == HttpStatusCode.Conflict) return Conflict(body);
            return StatusCode((int)resp.StatusCode, body);
        }

        // DELETE: /companies/{stockCode}  (Sil)
        [HttpDelete("/companies/{stockCode}")]
        public async Task<IActionResult> Delete(string stockCode)
        {
            if (string.IsNullOrWhiteSpace(stockCode))
                return BadRequest(new { message = "Geçersiz stok kodu." });

            var client = CreateDbClient();
            var url = $"/api/companies/{Uri.EscapeDataString(stockCode)}";
            Console.WriteLine($"→ DELETE {client.BaseAddress}{url}");
            var resp = await client.DeleteAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"← {resp.StatusCode} {resp.RequestMessage?.RequestUri}");
            Console.WriteLine($"← Body: {body}");

            if (resp.IsSuccessStatusCode) return Ok(new { message = "deleted" });
            if (resp.StatusCode == HttpStatusCode.NotFound) return NotFound(body);
            return StatusCode((int)resp.StatusCode, body);
        }
    }
}
