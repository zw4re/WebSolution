using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.DbModels;
using Microsoft.Extensions.Configuration;

namespace Worker.Services
{
    public class TcmbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // EVDS API anahtarım
        private readonly string _dbServiceUrl; // DatabaseService API url'si

        public TcmbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Evds:ApiKey"];
            _dbServiceUrl = configuration["URL:DatabaseService"];
        }

        public async Task FetchAndSendTcmbRatesAsync()
        {
            Console.WriteLine("TCMB döviz verisi çekiliyor...");

            string series = "TP.DK.USD.S-TP.DK.USD.A-TP.DK.EUR.S-TP.DK.EUR.A-TP.DK.CHF.S-TP.DK.CHF.A-TP.DK.GBP.S-TP.DK.GBP.A-TP.DK.JPY.S-TP.DK.JPY.A";
            string startDate = "01-10-2017";
            string endDate = "01-11-2017";

            string url = $"https://evds2.tcmb.gov.tr/service/evds/series={series}&startDate={startDate}&endDate={endDate}&type=json";

            _httpClient.DefaultRequestHeaders.Clear(); // Header temizlendi
            _httpClient.DefaultRequestHeaders.Add("key", _apiKey); // API Key eklendi
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            try
            {
                var response = await _httpClient.GetAsync(url); // Urlye get isteği gönderildi

                // API hata kontrol
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"TCMB API hatası: {response.StatusCode}");
                    Console.WriteLine(errorContent);
                    return;
                }

                var content = await response.Content.ReadAsStringAsync(); // Response içeriği string olarak alındı
                using var doc = JsonDocument.Parse(content); //JSONa parse edildi
                var items = doc.RootElement.GetProperty("items"); // items diziye erişildi

                var rates = new List<TcmbExchangeRate>(); // Verilerin tutulacağı boş liste 

                foreach (var item in items.EnumerateArray())
                {
                    var tarihStr = item.GetProperty("Tarih").GetString();
                    if (!DateTime.TryParseExact(tarihStr, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        continue;

                    foreach (var prop in item.EnumerateObject())
                    {
                        if (prop.Name.StartsWith("TP_DK") && decimal.TryParse(prop.Value.GetString()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                        {
                            var parts = prop.Name.Split('_');
                            if (parts.Length < 4) continue;

                            string currency = parts[2];
                            string type = parts[3] == "S" ? "Sell" : "Buy";

                            rates.Add(new TcmbExchangeRate // Parse edilen veri TcmbExchanceRate nesnesine dönüştürülür
                            {
                                Date = date,
                                CurrencyCode = $"{currency}/YTL",
                                Type = type,
                                Value = value
                            });
                        }
                    }
                }
                // Veri kontrol noktası
                if (rates.Count == 0)
                {
                    Console.WriteLine("İşlenecek döviz verisi bulunamadı.");
                    return;
                }
                // Veri gönderme 
                var postResponse = await _httpClient.PostAsJsonAsync($"{_dbServiceUrl}/api/tcmb", rates);
                if (postResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Veriler başarıyla DatabaseService'e gönderildi.");
                }
                else
                {
                    Console.WriteLine($"Veri gönderilemedi. Status: {postResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TCMB servisinde hata oluştu: " + ex.Message);
            }
        }
    }
}
