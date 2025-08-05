using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace AdminAPI.Services
{
    // Redis'e bağlanıp job bilgilerini okuyacak servis sınıfı
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis; // Redis bağlantısını yöneten sınıf
        private readonly IDatabase _db;                // Redis veritabanı

        // Constructor: Redis'e bağlanır
        public RedisService()
        {
            // Bağlantı adresi (senin Redis adresin: localhost:6379)
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
        }

        // Job adı verildiğinde, Redis'ten job bilgisini çeker
        public async Task<JobInfo?> GetJobInfoAsync(string jobKey)
        {
            // Redis'teki job verisi genelde {hangfire}:recurring-job:{jobKey} şeklinde tutulur
            string redisKey = $"hangfire:recurring-job:{jobKey}";

            // Hash olarak kayıtlı verileri çek
            HashEntry[] hashEntries = await _db.HashGetAllAsync(redisKey);

            if (hashEntries.Length == 0)
                return null; // Kayıt yoksa null döner

            // Hash'i dictionary gibi parçalayıp modelimize atıyoruz
            var jobInfo = new JobInfo();

            foreach (var entry in hashEntries)
            {
                switch (entry.Name.ToString())
                {
                    case "CreatedAt":
                        jobInfo.CreatedAt = entry.Value.ToString();
                        break;
                    case "LastExecution":
                        jobInfo.LastExecution = entry.Value.ToString();
                        break;
                    case "Job":
                        jobInfo.JobType = entry.Value.ToString();
                        break;
                    case "Cron":
                        jobInfo.Cron = entry.Value.ToString();
                        break;
                    case "NextExecution":
                        jobInfo.NextExecution = entry.Value.ToString();
                        break;
                }
            }

            return jobInfo;
        }
    }

    // Redis'ten çekilen job bilgilerini tutmak için sade bir model
    public class JobInfo
    {
        public string? CreatedAt { get; set; }
        public string? LastExecution { get; set; }
        public string? NextExecution { get; set; }
        public string? Cron { get; set; }
        public string? JobType { get; set; }
    }
}
