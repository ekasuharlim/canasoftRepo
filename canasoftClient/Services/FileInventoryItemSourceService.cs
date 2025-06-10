using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Services;
public class FileInventoryItemSource : IItemSource<CreateInventoryItemRequest>
{
    private readonly string _filePath;

    public FileInventoryItemSource(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IEnumerable<CreateInventoryItemRequest>> LoadAsync()
    {
        var lines = await File.ReadAllLinesAsync(_filePath);
        var items = new List<CreateInventoryItemRequest>();

        foreach (var line in lines.Skip(1)) 
        {
            var parts = line.Split(',');

            if (parts.Length < 7)
                continue;

            items.Add(new CreateInventoryItemRequest
            {
                CompanyCode = "CN001",
                ItemId = parts[0],
                ItemName = parts[1],
                WarehouseId = parts[2],
                WarehouseName = parts[3],
                Quantity = decimal.Parse(parts[4]),
                GroupItemId = parts[5],
                GroupItemName = parts[6],
                ItemCode = parts.Length > 7 ? parts[7] : null
            });
        }

        return items;  
    }

}