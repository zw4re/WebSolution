using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseService.Context;
using Entities.DbModels;


namespace DatabaseService.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CompaniesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _db.Companies.ToListAsync();
            return Ok(companies);
        }

        [HttpGet("{stockCode}")]
        public async Task<IActionResult> GetById(string stockCode)
        {
            var company = await _db.Companies.FindAsync(stockCode);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Company company)
        {
            try
            {
                if (company == null)
                {
                    Console.WriteLine("company objesi null geldi.");
                    return BadRequest("company null");
                }

                if (string.IsNullOrWhiteSpace(company.StockCode))
                {
                    Console.WriteLine("StockCode null veya boş.");
                    return BadRequest("StockCode boş olamaz");
                }

                // Burası patlıyorsa sebebini gör
                var sameRecordExists = await _db.Companies.AnyAsync(c =>
                    c.StockCode == company.StockCode &&
                    c.KapMemberTitle == company.KapMemberTitle &&
                    c.RelatedMemberTitle == company.RelatedMemberTitle &&
                    c.CityName == company.CityName &&
                    c.KapMemberType == company.KapMemberType &&
                    c.MkkMemberOid == company.MkkMemberOid &&
                    c.RelatedMemberOid == company.RelatedMemberOid
                );

                if (sameRecordExists)
                {
                    Console.WriteLine($"Aynı kayıt zaten var: {company.StockCode}");
                    return Conflict($"Aynı kayıt zaten mevcut: {company.StockCode}");
                }

                _db.Companies.Add(company);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { stockCode = company.StockCode }, company);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ Controller içi hata: " + ex.Message);
                return StatusCode(500, "Sunucu hatası: " + ex.Message);
            }
        }



        [HttpPut("{stockCode}")]
        public async Task<IActionResult> Update(string stockCode, [FromBody] Company updated)
        {
            var existing = await _db.Companies.FindAsync(stockCode);
            if (existing == null)
                return NotFound();

            existing.KapMemberTitle = updated.KapMemberTitle;
            existing.RelatedMemberTitle = updated.RelatedMemberTitle;
            existing.StockCode = updated.StockCode;
            existing.CityName = updated.CityName;
            existing.RelatedMemberOid = updated.RelatedMemberOid;
            existing.KapMemberType = updated.KapMemberType;
            existing.MkkMemberOid = updated.MkkMemberOid;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{stockCode}")]
        public async Task<IActionResult> Delete(string stockCode)
        {
            var existing = await _db.Companies.FindAsync(stockCode);
            if (existing == null)
                return NotFound();

            _db.Companies.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        //dashboard ekranındaki kap verileri şirket sayısı
        [HttpGet("count")]
        public async Task<IActionResult> GetCompanyCount()
        {
            var count = await _db.Companies.CountAsync();
            return Ok(count);
        }
        //düzeltilmiş parse işlemi
        [HttpPost("upsert")]
        public async Task<IActionResult> Upsert([FromBody] Company input)
        {
            Console.WriteLine($"[UPSERT] body alındı: StockCode='{input?.StockCode}', Name='{input?.KapMemberTitle}'");

            if (input == null || string.IsNullOrWhiteSpace(input.StockCode))
            {
                Console.WriteLine("[UPSERT] Geçersiz istek");
                return BadRequest("Geçersiz istek.");
            }

            var existing = await _db.Companies.FindAsync(input.StockCode);
            if (existing == null)
            {
                _db.Companies.Add(input);
                var affected = await _db.SaveChangesAsync();
                Console.WriteLine($"[UPSERT] CREATED '{input.StockCode}' (affected={affected})");
                return Ok(new { status = "created", stockCode = input.StockCode, affected });
            }

            existing.MkkMemberOid = input.MkkMemberOid;
            existing.KapMemberTitle = input.KapMemberTitle;
            existing.RelatedMemberTitle = input.RelatedMemberTitle;
            existing.CityName = input.CityName;
            existing.RelatedMemberOid = input.RelatedMemberOid;
            existing.KapMemberType = input.KapMemberType;

            var affectedUpdate = await _db.SaveChangesAsync();
            Console.WriteLine($"[UPSERT] UPDATED '{input.StockCode}' (affected={affectedUpdate})");
            return Ok(new { status = "updated", stockCode = input.StockCode, affected = affectedUpdate });
        }

    }
}