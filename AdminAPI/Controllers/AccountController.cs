using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json; // BU ÖNEMLİ!
using System;

namespace AdminAPI.Controllers // doğru namespace kullan!
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /login → Login sayfasını açar
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View(); // Views/Account/Login.cshtml
        }

        // POST: /login → Formdan gelen kullanıcı bilgilerini kontrol eder
        [HttpPost("/login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Giriş bilgilerini taşıyan anonim model
            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            // appsettings.json'dan gelen database API adresi
            var databaseServiceUrl = _configuration["URL:DatabaseService"];

            if (string.IsNullOrWhiteSpace(databaseServiceUrl))
            {
                ViewBag.Error = "Sunucu yapılandırması eksik.";
                return View();
            }

            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.PostAsJsonAsync($"{databaseServiceUrl}/api/auth/validate-admin", loginRequest);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
                    return View();
                }

                // Cookie Authentication oluştur
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Başarılı giriş sonrası yönlendirme
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Sunucuya bağlanılamadı. Lütfen daha sonra tekrar deneyin.";
                Console.WriteLine($"Login error: {ex.Message}");
                return View();
            }
        }

        // GET: /logout → Çıkış işlemi
        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}
