using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Abstractions;

public interface ISalesItemApiClient
{
    Task CreateSalesItemAsync(CreateSalesItemRequest request);
}

