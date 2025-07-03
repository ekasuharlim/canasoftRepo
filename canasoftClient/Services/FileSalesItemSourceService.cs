using System.Globalization;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Services;
public class FileSalesItemSourceService : IItemSource<CreateSalesItemRequest>
{
    private readonly ILogger<FileSalesItemSourceService> _logger;
    private string _spliter;

    public FileSalesItemSourceService(ILogger<FileSalesItemSourceService> logger, string spliter = ";")
    {
        _logger = logger;
        _spliter = spliter; 
    }

    public async Task<IEnumerable<CreateSalesItemRequest>> LoadAsync(string filePath)
    {
        _logger.LogInformation("Loading sales items from {FilePath}", filePath);
        var lines = await File.ReadAllLinesAsync(filePath);
        var salesItems = new List<CreateSalesItemRequest>();

        foreach (var line in lines.Skip(1)) 
        {
            var parts = line.Split(_spliter);

            if (parts.Length < 11)
            {
                _logger.LogWarning("Skipping malformed sales line: {Line}", line);
                continue;
            }

            try
            {
                if (!DateTime.TryParse(parts[0], out var salesDate))
                {
                    _logger.LogWarning("Skipping sales line due to invalid SalesDate: {Line}", line);
                    continue;
                }

                if (!decimal.TryParse(parts[9], NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity))
                {
                    _logger.LogWarning("Skipping sales line due to invalid Quantity: {Line}", line);
                    continue;
                }

                if (!decimal.TryParse(parts[10], NumberStyles.Number, CultureInfo.InvariantCulture, out var subTotal))
                {
                    _logger.LogWarning("Skipping sales line due to invalid SubTotal: {Line}", line);
                    continue;
                }

                var salesItem = new CreateSalesItemRequest
                {
                    CompanyCode = "CN001", 
                    SalesDate = salesDate,
                    ItemId = parts[1],
                    ItemName = parts[2],
                    ItemCode = parts[3],
                    GroupItemId = parts[4],
                    GroupItemName = parts[5],
                    WarehouseId = parts[6],
                    WarehouseName = parts[7],
                    SalesName = parts[8],
                    Quantity = quantity,
                    SubTotal = subTotal
                };

                salesItems.Add(salesItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing sales item from line: {Line}", line);
            }
        }
        _logger.LogInformation("Successfully loaded {ItemCount} sales items from {FilePath}", salesItems.Count, filePath);
        return salesItems;  
    }

}