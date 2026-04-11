using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        //services.AddApplicationInsightsTelemetryWorkerService();
        //services.ConfigureFunctionsApplicationInsights();

        string cosmosConn = context.Configuration["CosmosDbConnection"]!;
        services.AddSingleton(s =>
        {
            var options = new CosmosClientOptions
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = ConnectionMode.Gateway
            };
            return new CosmosClient(cosmosConn, options);
        });

        string storageConn = context.Configuration["AzureWebJobsStorage"]!;
        services.AddSingleton(x => new BlobServiceClient(storageConn));
    })
    .Build();

host.Run();