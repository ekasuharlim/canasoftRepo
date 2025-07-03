using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Factories;
public class FileProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public FileProcessorFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public (object source, object apiClient, string typeName)? GetProcessorForFile(string fileName)
    {
        var mappings = _configuration.GetSection("FileMappings").GetChildren();

        foreach (var mapping in mappings)
        {
            var prefix = mapping.Value;
            if (fileName.StartsWith(prefix!, StringComparison.OrdinalIgnoreCase))
            {
                switch (prefix)
                {
                    case "Inventory":
                        return (
                            _serviceProvider.GetRequiredService<IItemSource<CreateInventoryItemRequest>>(),
                            _serviceProvider.GetRequiredService<IItemApiClient<CreateInventoryItemRequest>>(),
                            "Inventory"
                        );

                    case "Sales":
                        return (
                            _serviceProvider.GetRequiredService<IItemSource<CreateSalesItemRequest>>(),
                            _serviceProvider.GetRequiredService<IItemApiClient<CreateSalesItemRequest>>(),
                            "Sales"
                        );
                }
            }
        }

        return null;
    }
}