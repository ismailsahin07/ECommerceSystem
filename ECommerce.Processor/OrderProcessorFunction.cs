using Azure.Storage.Blobs;
using ECommerce.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ECommerce.Processor
{
    public class OrderProcessorFunction
    {
        private readonly ILogger<OrderProcessorFunction> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly BlobServiceClient _blobServiceClient;

        private const string DatabaseName = "ECommerceDb";
        private const string ContainerName = "Orders";
        private const string BlobContainerName = "receipts";

        public OrderProcessorFunction(ILogger<OrderProcessorFunction> logger, CosmosClient cosmosClient, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _blobServiceClient = blobServiceClient;
        }

        [Function(nameof(OrderProcessorFunction))]
        public async Task Run([ServiceBusTrigger("orders-queue", Connection = "ServiceBusConnection")] string myQueueItem) 
        {
            _logger.LogInformation("Processing new order message from Service Bus.");

            var order = JsonSerializer.Deserialize<OrderRequest>(myQueueItem);
            if (order == null) return;

            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName);
            Container container = await database.CreateContainerIfNotExistsAsync(ContainerName, "/OrderId");

            var orderDocument = new
            {
                id = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                CustomerEmail = order.CustomerEmail,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                OrderDate = order.OrderDate
            };

            await container.CreateItemAsync(orderDocument, new PartitionKey(order.OrderId));
            _logger.LogInformation($"Successfully saved order {order.OrderId} to CosmosDb");

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            string receiptContent = $"--- OFFICIAL RECEIPT ---" +
                $"\nOrder ID: {order.OrderId}" +
                $"\nOrder Date: {order.OrderDate}" +
                $"\nProduct ID: {order.ProductId}" +
                $"\nQuantity: {order.Quantity}" +
                $"\nStatus: Paid and Processing...";

            BlobClient blobClient = blobContainerClient.GetBlobClient($"receipt-{order.OrderId}.txt");

            await blobClient.UploadAsync(BinaryData.FromString(receiptContent), overwrite: true);
            _logger.LogInformation($"Successfully uploaded receipt for order {order.OrderId} to Blob Storage");
        }
    }
}
