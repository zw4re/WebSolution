using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Admin.Controllers
{
    // Kullanıcı giriş işlemlerini yöneten controller
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Account/Login
        // Kullanıcıya giriş ekranını gösterir
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var apiUrl = _configuration["URL:DatabaseService"];
                var client = _httpClientFactory.CreateClient();
                var payload = new { username, password };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Cookie Authentication başlat
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return Redirect("/dashboard");
                }
                else
                {
                    ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
                    return View();
                }
            }
            catch
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
                return View();
            }
        }
    }
}