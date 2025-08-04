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

        // GET: /api/companies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _db.Companies.ToListAsync();
            return Ok(companies);
        }

        // GET: /api/companies/{stockCode}
        [HttpGet("{stockCode}")]
        public async Task<IActionResult> GetById(string stockCode)
        {
            var company = await _db.Companies.FindAsync(stockCode);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        // POST: /api/companies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Company company)
        {
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            // StockCode birincil anahtar olduğu için onu kullanıyoruz
            return CreatedAtAction(nameof(GetById), new { stockCode = company.StockCode }, company);
        }

        // PUT: /api/companies/{stockCode}
        [HttpPut("{stockCode}")]
        public async Task<IActionResult> Update(string stockCode, [FromBody] Company updated)
        {
            var existing = await _db.Companies.FindAsync(stockCode);
            if (existing == null)
                return NotFound();

            // Güncellenebilir alanlar
            existing.KapMemberTitle = updated.KapMemberTitle;
            existing.RelatedMemberTitle = updated.RelatedMemberTitle;
            existing.CityName = updated.CityName;
            existing.RelatedMemberOid = updated.RelatedMemberOid;
            existing.KapMemberType = updated.KapMemberType;
            existing.MkkMemberOid = updated.MkkMemberOid;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: /api/companies/{stockCode}
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
    }
}
