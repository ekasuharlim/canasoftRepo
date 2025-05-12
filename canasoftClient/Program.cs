
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
            services.AddHttpClient<IInventoryApiClient, InventoryApiClientService>(client =>
            {
                client.BaseAddress = new Uri("https://myapp.local/");
            })
            .ConfigurePrimaryHttpMessageHandler( () => {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var handler = new HttpClientHandler();
                if (env != null && env.Equals("development", StringComparison.InvariantCultureIgnoreCase)) {
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
                return handler;
            }
        );
    })
    .Build();

var apiClient = host.Services.GetRequiredService<IInventoryApiClient>();
await apiClient.GetAllItemAsync();
await apiClient.CreateItemAsync();