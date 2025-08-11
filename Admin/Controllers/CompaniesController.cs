using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Admin.Controllers
{
    [ApiController]
    [Route("api/admin/companies")]           // benzersiz rota
    public class CompaniesController : ControllerBase
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;
        public CompaniesController(IHttpClientFactory http, IConfiguration cfg)
        { _http = http; _cfg = cfg; }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var baseUrl = _cfg["URL:DatabaseService"];
            var res = await _http.CreateClient().GetAsync($"{baseUrl}/api/companies");
            var body = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, body);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.StockCode))
                return BadRequest("StockCode boş olamaz");

            var baseUrl = _cfg["URL:DatabaseService"];
            var payload = new
            {
                kapMemberTitle = dto.Name,
                stockCode = dto.StockCode,
                cityName = dto.City,
                kapMemberType = dto.Type,
                relatedMemberTitle = dto.RelatedMemberTitle,
                mkkMemberOid = dto.MkkMemberOid,
                relatedMemberOid = dto.RelatedMemberOid
            };

            var res = await _http.CreateClient().PostAsJsonAsync($"{baseUrl}/api/companies", payload);
            var body = await res.Content.ReadAsStringAsync();
            return StatusCode((int)res.StatusCode, body);
        }
    }

    public class CreateCompanyDto
    {
        public string Name { get; set; }
        public string StockCode { get; set; }
        public string City { get; set; }
        public string Type { get; set; }
        public string? RelatedMemberTitle { get; set; }
        public string? MkkMemberOid { get; set; }
        public string? RelatedMemberOid { get; set; }
    }
}
