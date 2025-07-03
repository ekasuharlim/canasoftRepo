using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CanasoftClient.Config;


namespace CanasoftClient.Processor;

public class InventoryFileProcessor : FileProcessorBase<CreateInventoryItemRequest>
{
    public InventoryFileProcessor(
        IItemSource<CreateInventoryItemRequest> source,
        IItemApiClient<CreateInventoryItemRequest> apiClient,
        IOptionsMonitor<FileMappingConfig> optionsMonitor)
        : base(source, apiClient, optionsMonitor.Get("Inventory")) { }
}