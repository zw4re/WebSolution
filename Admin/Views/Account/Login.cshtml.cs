using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Admin.Views.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            using var client = new HttpClient();
            // DatabaseService API adresini buraya yaz
            var apiUrl = "http://localhost:5001/api/Login"; // Portunu kendi DatabaseService'ine göre ayarla

            var loginRequest = new
            {
                Username,
                Password
            };

            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Giriþ baþarýlý, dashboard'a yönlendir
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Kullanýcý adý veya þifre hatalý!";
                return Page();
            }
        }
    }
}