using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Worker.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory; // API çağrısı için HttpClient factory
        private readonly IConfiguration _configuration;         // appsettings.json'dan ayarları okumak için

        // constructor ile bağımlılıklar enjekte ediliyor
        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET /login > Login sayfasını gösterir
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View(); // Login.cshtml sayfasını döner
        }

        // POST /login > Kullanıcı adı ve şifre alıp API üzerinden doğrular
        [HttpPost("/login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var client = _httpClientFactory.CreateClient(); // HttpClient örneği oluştur

            // databaseService API URL'sini appsettings.json'dan alıyoruz
            var loginUrl = _configuration["URL:DatabaseService"] + "/api/login";

            // Şifreyi hash'lemeden doğrudan gönderiyoruz
            var response = await client.PostAsJsonAsync(loginUrl, new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                // API başarılı dönerse kullanıcıyı sisteme giriş yaptırıyoruz
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Cookie ile oturumu başlat
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Giriş başarılı ise Hangfire dashboard'a yönlendir
                return Redirect("/hangfire");
            }

            // Hatalı girişte kullanıcıya mesaj göster
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // GET /logout => Oturumu kapat ve login sayfasına yönlendir
        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/login");
        }
    }
}
