using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Entities.DbModels;
using DatabaseService.Context; 
using Microsoft.EntityFrameworkCore;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    // Constructor: HttpClient ve appsettings'e erişim için gerekli nesneleri alıyoruz
    public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // GET: /login → Login sayfasını açar
    [HttpGet("/login")]
    public IActionResult Login()
    {
        return View(); // Views/Account/Login.cshtml dosyasını açar
    }

    // POST: /login → Formdan gelen kullanıcı bilgilerini kontrol eder
    [HttpPost("/login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Giriş bilgilerini taşıyacak veri modeli (JSON olarak yollanacak)
        var loginRequest = new
        {
            Username = username,
            Password = password
        };

        // appsettings.json içindeki DatabaseService adresini alıyoruz
        var databaseServiceUrl = _configuration["URL:DatabaseService"];

        // İstek atmak için HttpClient oluştur
        var client = _httpClientFactory.CreateClient();

        // JSON içeriği hazırlıyoruz
        var content = JsonContent.Create(loginRequest);

        // /api/auth/validate-admin endpoint'ine POST isteği gönder
        var response = await client.PostAsync($"{databaseServiceUrl}/api/auth/validate-admin", content);

        // Kullanıcı doğrulaması başarısızsa
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
            return View();
        }

        // Eğer giriş başarılıysa → Cookie oluştur
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Kullanıcıyı sisteme giriş yapmış olarak işaretle
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Başarılı giriş sonrası admin dashboard'a yönlendir
        return RedirectToAction("Index", "AdminDashboard");
    }

    // GET: /logout → Kullanıcı çıkış yaparsa oturumu kapat
    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}

