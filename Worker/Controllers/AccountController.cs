using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Worker.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View(); // Login.cshtml açılır
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Örnek kullanıcı kontrolü
            if (username == "admin" && password == "1234")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                
                // kullanıcıyı sisteme giriş yaptırır
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                // giriş başarılıysa doğrudan Hangfire dashboard sayfasına yönlendirilir
                return Redirect("/hangfire");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/login");
        }
    }
}