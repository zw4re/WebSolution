using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using RabbitMQ.Client;

namespace CommonMessaging
{
    public class RabbitPublisher
    {

        private readonly IConnection _connection;
        //RabbitConnection.cs nesnesi -> rabbitConnection'a aktarıldı 
        public RabbitPublisher(RabbitConnection rabbitConnection)
        {
            _connection = rabbitConnection.Connection // RabbitConnection içindeki property
                ?? throw new ArgumentNullException(nameof(rabbitConnection), "Bağlantı boş olamaz.");
        }

        public void Publish<T>(string exchange, string routingKey, T message)
        {
            // yeni kanal açma
            using var channel = _connection.CreateModel();

            //exchance yoksa oluşturucak
            channel.ExchangeDeclare(exchange: exchange, type: "direct", durable: true);
            // Mesajı JSON'a çevir
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // Mesaj özellikleri 

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            // mesaj gönderme komutu 
            channel.BasicPublish(
                   exchange: exchange,
                   routingKey: routingKey,
                   basicProperties: properties,
                   body: body
               );

            Console.WriteLine($"[Publisher] Mesaj gönderildi → Exchange: {exchange}, RoutingKey: {routingKey}");

        }
    }
}

