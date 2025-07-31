using Microsoft.AspNetCore.Mvc;
using Entities.DbModels;
using DatabaseService.Context;
using Microsoft.EntityFrameworkCore;

namespace DatabaseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TcmbController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TcmbController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tüm verileri getir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TcmbExchangeRate>>> GetAll()
        {
            return await _context.TcmbExchangeRates.ToListAsync();
        }

        // GET: Belirli veriyi getir (birleşik key ile)
        [HttpGet("{date}/{currencyCode}/{type}")]
        public async Task<ActionResult<TcmbExchangeRate>> GetByKey(DateTime date, string currencyCode, string type)
        {
            var rate = await _context.TcmbExchangeRates.FindAsync(date, currencyCode, type);

            if (rate == null)
                return NotFound();

            return rate;
        }

        // POST: Yeni veri ekle 
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<TcmbExchangeRate> rates)
        {
            if (rates == null || !rates.Any())
                return BadRequest("Veri listesi boş.");

            foreach (var rate in rates)
            {
                var existing = await _context.TcmbExchangeRates.FindAsync(rate.Date, rate.CurrencyCode, rate.Type);
                if (existing == null)
                {
                    _context.TcmbExchangeRates.Add(rate);
                }
                else
                {
                    existing.Value = rate.Value;
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Veriler kaydedildi.");
        }

        // PUT: Belirli kaydı güncelle
        [HttpPut("{date}/{currencyCode}/{type}")]
        public async Task<IActionResult> Put(DateTime date, string currencyCode, string type, [FromBody] TcmbExchangeRate updated)
        {
            if (updated == null || (updated.Date != date || updated.CurrencyCode != currencyCode || updated.Type != type))
                return BadRequest("Parametreler ve veri uyumsuz.");

            var existing = await _context.TcmbExchangeRates.FindAsync(date, currencyCode, type);
            if (existing == null)
                return NotFound();

            existing.Value = updated.Value;

            await _context.SaveChangesAsync();
            return Ok("Kayıt güncellendi.");
        }

        // DELETE: Belirli kaydı sil
        [HttpDelete("{date}/{currencyCode}/{type}")]
        public async Task<IActionResult> Delete(DateTime date, string currencyCode, string type)
        {
            var rate = await _context.TcmbExchangeRates.FindAsync(date, currencyCode, type);
            if (rate == null)
                return NotFound();

            _context.TcmbExchangeRates.Remove(rate);
            await _context.SaveChangesAsync();
            return Ok("Kayıt silindi.");
        }
    }
}
