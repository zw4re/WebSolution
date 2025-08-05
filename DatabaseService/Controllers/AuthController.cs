using Microsoft.AspNetCore.Mvc;
using DatabaseService.Context;
using Entities.Presentation;



namespace DatabaseService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("validate-admin")]
        public IActionResult ValidateAdmin([FromBody] AdminLoginRequest request)
        {
            Console.WriteLine($"GELEN VERİ: username={request.Username}, password={request.Password}");

            var admin = _context.AdminUsers
                .FirstOrDefault(x => x.Username == request.Username && x.Password == request.Password);

            if (admin == null)
            {
                Console.WriteLine("KULLANICI BULUNAMADI!");
                return Unauthorized("Geçersiz kullanıcı adı veya şifre");
            }

            Console.WriteLine("KULLANICI DOĞRULANDI");
            return Ok(true);
        }

    }
}
