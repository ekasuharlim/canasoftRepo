using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CanasoftClient.Config;

namespace CanasoftClient.Processor;

public abstract class FileProcessorBase<TRequest> : IFileProcessor
{
    private readonly IItemSource<TRequest> _source;
    private readonly IItemApiClient<TRequest> _apiClient;
    private readonly FileMappingConfig _config;

    protected FileProcessorBase(
        IItemSource<TRequest> source,
        IItemApiClient<TRequest> apiClient,
        FileMappingConfig config)
    {
        _source = source;
        _apiClient = apiClient;
        _config = config;
    }

    public string FilePrefix => _config.FilePrefix;
    public string TypeName => _config.TypeName;

    public bool CanProcess(string fileName) =>
        fileName.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase);

    public async Task ProcessAsync(string filePath, ILogger logger)
    {
        logger.LogInformation("Start sending to {ItemTypeName} API", TypeName);
        var items = await _source.LoadAsync(filePath);
        foreach (var item in items)
        {
            try
            {
                await _apiClient.CreateItemAsync(item);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on loading {ItemTypeName} item: {Item}", TypeName, item);
            }
        }

        logger.LogInformation("Loaded {ItemCount} {ItemTypeName} items from {FilePath}.", items.Count(), TypeName, filePath);
    }
}