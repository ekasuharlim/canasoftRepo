using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Abstractions;

public interface IInventoryItemSource
{
    Task<IEnumerable<CreateInventoryItemRequest>> LoadAsync();
}