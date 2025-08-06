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

        [HttpGet("last-date")]
        public async Task<IActionResult> GetLastExchangeRateDate()
        {
            // veritabanındaki tcmb_exchange_rate tablosundan en son eklenen tarihi bulur
            var lastDate = await _context.TcmbExchangeRates
                .OrderByDescending(x => x.Date) // Tarihe göre büyükten küçüğe sırala
                .Select(x => x.Date)            // Sadece tarih alanını al
                .FirstOrDefaultAsync();         // En üstteki (yani en güncel) tarihi al

            // veritabanında hiç veri yoksa default değer gelir > 01.01.0001
            if (lastDate == default)
                // bu durumda 2015-01-01 string olarak döndürülür başlangıç tarihi olarak
                return Ok("2000-01-01");

            // veri varsa, en son tarihi "yyyy-MM-dd" formatında string olarak döndür
            return Ok(lastDate.ToString("yyyy-MM-dd"));
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

            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine("HATA: " + ex.Message);

                // Daha fazla bilgi görmek için inner exception'ı yazdıralım:
                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Veri kaydedilirken bir hata oluştu: " + ex.Message);
            }

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

        // Dashboard ekranındaki döviz kurları
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestExchangeRates()
        {
            // En güncel tarihi bul
            var latestDate = await _context.TcmbExchangeRates
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstOrDefaultAsync();

            // O tarihe ait ilk 5 kayıt
            var latestRates = await _context.TcmbExchangeRates
                .Where(x => x.Date == latestDate)
                .OrderBy(x => x.CurrencyCode) // İsteğe bağlı sıralama
                .ToListAsync();


            return Ok(latestRates);
        }
    }
}
