using Azure.Messaging.ServiceBus;
using ECommerce.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ECommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ServiceBusClient _serviceBusClient;
        private const string QueueName = "orders-queue";

        public OrdersController(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderRequest request)
        {
            if(request.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            await using ServiceBusSender sender = _serviceBusClient.CreateSender(QueueName);

            string messageBody = JsonSerializer.Serialize(request);

            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(message);

            return Accepted(new { Status = "Order received and queued for processing", OrderId = request.OrderId });
        }
    }
}
