using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMessaging
{
    // Mesajların ortak bilgileri
    // zarfın uzerınde yer alması gereken standart bilgiler gibi
    public record Envelope<T>(
        string MessageId,      // Her mesaj için benzersiz ID 
        string TraceId,        // Servisler arası zinciri takip etmek için
        string Type,           // Mesaj tipi örneğin "worker.runjob" or "job.complated"
        T Payload,             // Taşınan iş verisi
        DateTimeOffset SentAt  // Mesaj oluşturulma zamanı (UTC)
        );
}
