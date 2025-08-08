using Hangfire;
using Hangfire.Storage;
using Entities.Presentation; 
using System.Collections.Generic;

namespace Admin.Services
{
    public class RedisService
    {
        // Redis üzerindeki tanımlı tüm Recurring Jobları getirir
        public List<RecurringJobInfo> GetRecurringJobs()
        {
            // Geri döneceğimiz listeyi oluşturuyoruz
            var list = new List<RecurringJobInfo>();

            // Hangfire üzerinden mevcut Redis bağlantısını alır
            var storage = JobStorage.Current;
            if (storage == null) return list; // storage yoksa boş dön

            using var connection = storage.GetConnection();
            if (connection == null) return list; // connection alınamadıysa boş dön

            // Redis'te kayıtlı olan tüm Recurring Job'ları getirir
            var jobs = connection.GetRecurringJobs();
            if (jobs == null) return list; // job listesi yoksa boş dön

            // Her bir job için bilgileri RecurringJobInfo modeline atıyoruz
            foreach (var job in jobs)
            {
                if (job == null) continue;

                // Hangi sınıf ve metot çalışacak → null güvenli
                var typeName = job.Job?.Type?.Name ?? "-";
                var methodName = job.Job?.Method?.Name ?? "-";
                var jobName = $"{typeName}.{methodName}";

                list.Add(new RecurringJobInfo
                {
                    Id = job.Id ?? "-",                 // Job'un benzersiz ID'si
                    Cron = job.Cron ?? "-",             // Cron formatındaki zamanlama bilgisi
                    TimeZone = job.TimeZoneId ?? "-",   // Zaman dilimi bilgisi (örn: UTC)
                    Job = jobName,                      // Hangi sınıf ve metot çalışacak
                    LastExecution = job.LastExecution,  // En son çalıştırılma zamanı
                    NextExecution = job.NextExecution,  // Bir sonraki çalıştırılma zamanı
                    Status = job.LastJobState ?? "-"    // Job'un son durumu (örn: Succeeded, Failed vs.)
                });
            }

            return list;
        }
    }
}
