using System;
using RabbitMQ.Client;

namespace CommonMessaging
{
    public sealed class RabbitConnection : IDisposable
    {
        // RabbitMQ bağlantı nesnesi
        public IConnection Connection { get; }

        // MessagingOptions ile gelen RabbitMQ URIsini kullanarak bağlantı kurar
        public RabbitConnection(MessagingOptions opt)
        {
            if (opt == null)
                throw new ArgumentNullException(nameof(opt), "MessagingOptions boş olamaz.");

            // Bağlantı fabrikası oluşturma
            var factory = new ConnectionFactory
            {
                Uri = new Uri(opt.RabbitMqUri),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(30),
                ConsumerDispatchConcurrency = 1 
            };

            Connection = factory.CreateConnection(); 
        } 

        // Uygulama kapanırken bağlantıyı kapatır.
        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
