using Entities.DbModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    public class CompanyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _dbServiceUrl;

        public CompanyService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _dbServiceUrl = config["URL:DatabaseService"];
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            var companies = await _httpClient.GetFromJsonAsync<List<Company>>($"{_dbServiceUrl}/api/companies");
            return companies ?? new List<Company>();
        }

        public async Task<Company?> GetCompanyByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Company>($"{_dbServiceUrl}/api/companies/{id}");
        }
    }
}
