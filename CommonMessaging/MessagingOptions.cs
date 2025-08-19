using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMessaging
{
    public sealed class MessagingOptions
    {
        // RabbitMQ sunucusuna baglanmak için amqp URI'si
        public string RabbitMqUri { get; set; } = "amqp://user:pass@localhost:5672";
        public string ServiceName { get; set; } = ""; // Admin - Worker - DatabaseService
    }
}
