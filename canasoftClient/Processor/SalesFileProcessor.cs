using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CanasoftClient.Config;

namespace CanasoftClient.Processor;

public class SalesFileProcessor : IFileProcessor
{
    private readonly IItemSource<CreateSalesItemRequest> _source;
    private readonly IItemApiClient<CreateSalesItemRequest> _apiClient;
    private readonly FileMappingConfig _config;


    public string FilePrefix => _config.FilePrefix;
    public string TypeName =>  _config.TypeName;

    public SalesFileProcessor(
        IItemSource<CreateSalesItemRequest> source,
        IItemApiClient<CreateSalesItemRequest> apiClient,
        IOptionsMonitor<FileMappingConfig> optionsMonitor)
    {
        _source = source;
        _apiClient = apiClient;
        _config = optionsMonitor.Get("Sales");
    }

    public bool CanProcess(string fileName) =>
        fileName.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase);

    public async Task ProcessAsync(string filePath, ILogger logger)
    {
        logger.LogInformation("Start sending to Sales API");
        var items = await _source.LoadAsync(filePath);
        foreach (var item in items)
        {
            try
            {
                await _apiClient.CreateItemAsync(item);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on loading Sales item: {Item}", item);
            }
        }
        logger.LogInformation("Loaded {ItemCount} {ItemTypeName} items from {FilePath}.", items.Count(), TypeName, filePath);
    }
}