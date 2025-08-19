using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace CommonMessaging
{
    public static class MessagingBootstrapper
    {
        public static void EnsureTopology(RabbitConnection conn)
        {
            // conndaki RabbitConnection nesnesinin içinden Connectionu al onun uzerınen yeni kanal ac
            using var ch = conn.Connection.CreateModel();
            

            // Exchange
            ch.ExchangeDeclare(exchange: "app.commands", type: "direct", durable: true, autoDelete: false, arguments: null);
            ch.ExchangeDeclare(exchange: "app.events", type: "topic", durable: true, autoDelete: false, arguments: null);

            // Queue
            ch.QueueDeclare(queue: "worker.commands", durable: true, exclusive: false, autoDelete: false, arguments: null);
            ch.QueueDeclare(queue: "dbservice.events", durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Binding
            ch.QueueBind(queue: "worker.commands", exchange: "app.commands", routingKey: "worker.runJob.*", arguments: null);
            ch.QueueBind(queue: "dbservice.events", exchange: "app.events", routingKey: "job.*", arguments: null);
        }
    }
}
