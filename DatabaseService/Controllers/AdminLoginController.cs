using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DatabaseService.Context;
using DatabaseService.Entities;

namespace DatabaseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminLoginController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminLoginController(AppDbContext context)
        {
            _context = context;
        }

        // Giriş için model
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        // POST api/adminlogin
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.AdminApiUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
                return Unauthorized();

            return Ok(new { user.Username });
        }
    }
}
