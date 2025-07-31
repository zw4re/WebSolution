using System;
using System.Threading.Tasks;
using Worker.Services;

namespace Worker.Jobs
{
    public class TcmbJob
    {
        private readonly TcmbService _tcmbService; // Bağımlılık

        public TcmbJob(TcmbService tcmbService)
        {
            _tcmbService = tcmbService;
        }

        public async Task Run()
        {
            Console.WriteLine("TCMB Job başladı...");
            await _tcmbService.FetchAndSendTcmbRatesAsync();
            Console.WriteLine("TCMB Job tamamlandı.");
        }
    }
}
