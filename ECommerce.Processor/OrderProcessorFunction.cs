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
    }

    public class OrderProcessorFunction
    {
        private readonly ILogger<OrderProcessorFunction> _logger;

        public OrderProcessorFunction(ILogger<OrderProcessorFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderProcessorFunction))]
        public OrderProcessorOutputs Run(
            [ServiceBusTrigger("orders-queue", Connection = "ServiceBusConnection")] string myQueueItem)
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

            string receipt = $"--- OFFICIAL RECEIPT ---\nOrder ID: {order.OrderId}\nStatus: Paid";

            return new OrderProcessorOutputs
            {
                CosmosDocument = document,
                ReceiptContent = receipt
            };
        }
    }
}