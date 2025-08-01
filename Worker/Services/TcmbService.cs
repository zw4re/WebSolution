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

            string series = "TP.DK.USD.S.YTL-TP.DK.USD.A.YTL-TP.DK.EUR.S.YTL-TP.DK.EUR.A.YTL-TP.DK.CHF.S.YTL-TP.DK.CHF.A.YTL-TP.DK.GBP.S.YTL-TP.DK.GBP.A.YTL-TP.DK.JPY.S.YTL-TP.DK.JPY.A.YTL";
            // Veritabanındaki en son döviz verisi tarihini al
            var lastDate = await GetLastExchangeRateDateAsync(); // Örn: 2025-08-01

            // O tarihten 1 gün sonrasını startDate olarak belirle
            var startDate = lastDate.AddDays(1); // Örn: 2025-08-02

            // endDate = Bugünün tarihi
            var endDate = DateTime.Today; // Örn: 2025-08-03

            // sistem zaten güncelse ve veri çekilecek tarih yoksa, işlemi durdur
            if (startDate > endDate)
            {
                Console.WriteLine("Yeni veri yok, işlem durduruldu.");
                return;
            }

            // EVDS API için tarihi string formatına çevir (dd-MM-yyyy)
            string startDateStr = startDate.ToString("dd-MM-yyyy");
            string endDateStr = endDate.ToString("dd-MM-yyyy");

            // API URL’sini dinamik tarihlerle oluştur
            string url = $"https://evds2.tcmb.gov.tr/service/evds/series={series}&startDate={startDateStr}&endDate={endDateStr}&type=json";


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
        // veritabanında kayıtlı en son döviz kuru tarihini almak için kullanılır.
        private async Task<DateTime> GetLastExchangeRateDateAsync()
        {
            return new DateTime(2000, 1, 1);
        }
    }
}
