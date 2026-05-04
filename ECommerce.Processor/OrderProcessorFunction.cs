using Azure.Messaging;
using ECommerce.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ECommerce.Processor
{
    public class OrderProcessorOutputs
    {
        [CosmosDBOutput(databaseName: "ECommerceDb", containerName: "Orders", Connection = "CosmosDbConnection")]
        public object CosmosDocument { get; set; }

        [BlobOutput("receipts/receipt-{OrderId}.txt", Connection = "BlobStorageConnection")]
        public string ReceiptContent { get; set; }

        [EventGridOutput(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")]
        public CloudEvent PublishEvent { get; set; }
    }

    public class OrderProcessorData
    {
        public string OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public string ReceiptUri { get; set; }
    }

    public class OrderProcessorFunction
    {
        private readonly ILogger<OrderProcessorFunction> _logger;

        public OrderProcessorFunction(ILogger<OrderProcessorFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderProcessorFunction))]
        public OrderProcessorOutputs Run([ServiceBusTrigger("orders-queue", Connection = "ServiceBusConnection")] string myQueueItem)
        {
            _logger.LogInformation("Processing new order message.");

            var order = JsonSerializer.Deserialize<OrderRequest>(myQueueItem);
            if (order == null) return null;

            var document = new
            {
                id = order.OrderId,
                OrderId = order.OrderId,
                CustomerEmail = order.CustomerEmail,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                OrderDate = order.OrderDate
            };

            var eventData = new OrderProcessorData
            {
                OrderId = order.OrderId,
                CustomerEmail = order.CustomerEmail,
                ReceiptUri = $"https://yourstorage.blob.core.windows.net/receipts/receipt-{order.OrderId}.txt"
            };

            var cloudEvent = new CloudEvent(
                source: "ECommerce.Processor.OrderProcessorFunction",
                type: "Contoso.ECommerce.Processor",
                jsonSerializableData: eventData
            )
            {
                Subject = $"orders/processed/customers/{order.CustomerEmail}",
                DataContentType = "application/json"
            };

            string receipt = $"--- OFFICIAL RECEIPT ---\nOrder ID: {order.OrderId}\nStatus: Paid";

            return new OrderProcessorOutputs
            {
                CosmosDocument = document,
                ReceiptContent = receipt,
                PublishEvent = cloudEvent
            };
        }
    }
}