using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Services;
public class FileInventoryItemSource : IItemSource<CreateInventoryItemRequest>
{
    private readonly ILogger<FileInventoryItemSource> _logger;
    private string _spliter;

    public FileInventoryItemSource(ILogger<FileInventoryItemSource> logger, string spliter = ";")
    {
        _spliter = spliter;
        _logger = logger;
    }

    public async Task<IEnumerable<CreateInventoryItemRequest>> LoadAsync(string filePath)
    {
        _logger.LogInformation("Loading inventory items Eka from {FilePath}", filePath);

        var lines = await File.ReadAllLinesAsync(filePath);
        var items = new List<CreateInventoryItemRequest>();

        foreach (var line in lines.Skip(1)) 
        {
            var parts = line.Split(_spliter);

            if (parts.Length < 7)
            {
                _logger.LogWarning("Skipping malformed inventory line: {Line}", line);
                continue;
            }

            try
            {
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
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Error parsing inventory item from line: {Line}", line);
            }
        }
        _logger.LogInformation("Successfully loaded {ItemCount} inventory items from {FilePath}", items.Count, filePath);
        return items;  
    }

}