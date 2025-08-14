using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Entities;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Entities.Presentation;

namespace Worker.Services
{
    public class KapParseService
    {
        private readonly HttpClient _httpClient;

        // veriyi gönderdiğim API adresi
        private readonly string _dbServiceUrl;


        public KapParseService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _dbServiceUrl = config["URL:DatabaseService"];
        }

        public async Task FetchAndSendCompaniesAsync()
        {
            var url = "https://www.kap.org.tr/tr/bist-sirketler";
            var html = await _httpClient.GetStringAsync(url);

            var doc = new HtmlDocument(); // yeni html nesnesi oluşturdm
            doc.LoadHtml(html); // yukarıda indirdiğim htmli bu nesneye yükledim

            var scripts = doc.DocumentNode.SelectNodes("//script"); // html içindeki script etiketlerini dönecek
            string jsonString = null;

            //Doğru scripti bulma ve Json çıkartma 

            foreach (var script in scripts)
            {
                var content = script.InnerText;

                if (content.Contains("kapMemberTitle") && content.Contains("relatedMemberTitle"))
                {
                    // Köşeli parantez sayarak tam jsonu çıkarma
                    int start = content.IndexOf("[{", StringComparison.Ordinal);
                    if (start >= 0)
                    {
                        int bracket = 0;
                        int endIndex = -1;

                        for (int i = start; i < content.Length; i++)
                        {
                            char ch = content[i];
                            if (ch == '[') bracket++;
                            else if (ch == ']') bracket--;

                            if (bracket == 0) // sayaç eğer 0sa json stringin sonuna gelmiş oluyoruz
                            {
                                endIndex = i;
                                break;
                            }
                        }

                        if (endIndex > start)
                        {
                            jsonString = content.Substring(start, endIndex - start + 1);
                        }
                    }

                    // Fallback hala yoksa regex ile [ { … } ] yapıyı arar
                    if (jsonString == null)
                    {
                        var match = Regex.Match(content, @"\[\s*\{.*\}\s*\]", RegexOptions.Singleline);
                        if (match.Success)
                            jsonString = match.Value;
                    }

                    if (jsonString != null)
                        break;
                }
            }

            if (jsonString == null)
            {
                Console.WriteLine("Veri bulunamadı.");
                return;
            }

            Console.WriteLine("JSON STRING (ilk 500 karakter):");
            Console.WriteLine(jsonString.Substring(0, Math.Min(500, jsonString.Length)));

            try
            {
                
                var trimmed = jsonString.Trim();

                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
                {
                    // Dıştan tırnaklı "JSON string" ise gerçek metne çevirir
                    jsonString = JsonSerializer.Deserialize<string>(trimmed);
                }
                else if (jsonString.Contains("\\\"code\\\"") || jsonString.Contains("\\\"kapMemberTitle\\\""))
                {
                    // Dıştan tırnak yok ama içeride kaçışlar varsa temel kaçışları çözer
                    jsonString = jsonString
                        .Replace("\\\"", "\"")
                        .Replace("\\/", "/");
                }

                // Deserialize 
                var wrapperList = JsonSerializer.Deserialize<List<CompanyWrapperJsonModel>>(jsonString);

                if (wrapperList == null || wrapperList.Count == 0)
                {
                    Console.WriteLine("Veri çözümlenemedi.");
                    return;
                }

                var formattedJson = JsonSerializer.Serialize(wrapperList, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("Formatted JSON:");
                Console.WriteLine(formattedJson);

                foreach (var wrapper in wrapperList)
                {
                    foreach (var parsed in wrapper.content)
                    {
                        bool hasNullValue = parsed.GetType().GetProperties().Any(prop => prop.GetValue(parsed) == null);
                        if (hasNullValue) continue;

                        var company = new
                        {
                            MkkMemberOid = parsed.mkkMemberOid,
                            KapMemberTitle = parsed.kapMemberTitle,
                            RelatedMemberTitle = parsed.relatedMemberTitle,
                            StockCode = parsed.stockCode,
                            CityName = parsed.cityName,
                            RelatedMemberOid = parsed.relatedMemberOid,
                            KapMemberType = parsed.kapMemberType
                        };

                        var response = await _httpClient.PostAsJsonAsync($"{_dbServiceUrl}/api/companies", company);
                        if (!response.IsSuccessStatusCode)
                            Console.WriteLine($"Gönderim başarısız: {response.StatusCode}");
                    }
                }

                Console.WriteLine("Veriler başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }


    }
}