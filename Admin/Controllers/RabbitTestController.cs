using Microsoft.AspNetCore.Mvc;
using CommonMessaging;
using RabbitMQ.Client;
using System.Text;

namespace Admin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitTestController : ControllerBase
    {
        private readonly RabbitConnection _rabbitConnection;

        public RabbitTestController(RabbitConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
        }
        [HttpGet("send")]
        public IActionResult SendTestMessage()
        {
            // bağlantı uzerınden kanal açma
            using var channel = _rabbitConnection.Connection.CreateModel();
            channel.QueueDeclare(queue: "test-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            string message = $"Merhaba RabbitMQ! Zaman: {DateTime.Now}";
            var body = Encoding.UTF8.GetBytes(message);
            //mesajı kuyruğa gönderme
            channel.BasicPublish(exchange: "",
                                routingKey: "test-queue",
                                basicProperties: null,
                                body: body);

            return Ok($"Mesaj gönderildi: {message}");
        }
    }
}
