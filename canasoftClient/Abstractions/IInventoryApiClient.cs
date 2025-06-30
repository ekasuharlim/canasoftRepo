using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Abstractions;

public interface IInventoryApiClient : IItemApiClient<CreateInventoryItemRequest>
{
    Task GetAllInventoryItemAsync();
}

