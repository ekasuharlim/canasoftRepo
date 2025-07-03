using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CanasoftClient.Config;

namespace CanasoftClient.Processor;

public class SalesFileProcessor : FileProcessorBase<CreateSalesItemRequest>
{
    public SalesFileProcessor(
        IItemSource<CreateSalesItemRequest> source,
        IItemApiClient<CreateSalesItemRequest> apiClient,
        IOptionsMonitor<FileMappingConfig> optionsMonitor)
        : base(source, apiClient, optionsMonitor.Get("Sales")) { }
}
