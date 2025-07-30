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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _db.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Company company)
        {
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Company updated)
        {
            var existing = await _db.Companies.FindAsync(id);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _db.Companies.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Companies.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}