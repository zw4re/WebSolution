using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DatabaseService.Context;

namespace DatabaseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Constructor: AppDbContext, Dependency Injection ile sağlanıyor
        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // Kullanıcıdan gelen giriş verilerini tutan model
        public class LoginRequest
        {
            public string Username { get; set; }  // Kullanıcı adı
            public string Password { get; set; }  // Şifre (düz metin olarak)
        }

        // POST api/login - Giriş işlemi
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Request nesnesi ve zorunlu alanlar kontrolü
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                // Eksik ya da boş alan varsa 400 Bad Request döndürülür
                return BadRequest("Kullanıcı adı ve şifre gereklidir.");
            }

            // Veritabanından kullanıcı adı ve şifre (düz metin) eşleşen kullanıcıyı arıyoruz
            var user = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);


            if (user != null)
            {
                // Kullanıcı bulunduysa 200 OK ile başarılı giriş döner
                return Ok();
            }

            // Eşleşen kullanıcı bulunamadıysa 401 Unauthorized döner
            return Unauthorized();
        }
    }
}
