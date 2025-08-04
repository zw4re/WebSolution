using Microsoft.AspNetCore.Mvc;
using DatabaseService.Context;
using Presentation;


namespace DatabaseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // Admin kullanıcıyı kontrol etmek için POST endpoint
        [HttpPost("validate-admin")]
        public IActionResult ValidateAdmin([FromBody] AdminLoginRequest request)
        {
            // Veritabanında böyle bir kullanıcı var mı?
            var admin = _context.AdminUsers
                .FirstOrDefault(x => x.Username == request.Username && x.Password == request.Password);

            if (admin == null)
            {
                // Kullanıcı yoksa 401 hatası dön
                return Unauthorized("Geçersiz kullanıcı adı veya şifre");
            }

            // Varsa true dön (kabul edildi)
            return Ok(true);
        }
    }
}
