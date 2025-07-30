using System;
using System.Threading.Tasks;
using Worker.Services;

namespace Worker.Jobs
{
    public class KapJob
    {
        // kapparservice sınıfı bağımlılık olarak alındı 
        private readonly KapParseService _kapParseService;

        public KapJob(KapParseService kapParseService)
        {
            _kapParseService = kapParseService;
        }
        public async Task Run()
        {
            Console.WriteLine("KAP verileri çekiliyor...");
            try
            {
                await _kapParseService.FetchAndSendCompaniesAsync();
                Console.WriteLine("KAP veri işlemi tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"KAP job sırasında hata oluştu: {ex.Message}");
                throw; // Hangfire'ın job'ı başarısız olarak işaretlemesi için
            }
        }
    }
}
