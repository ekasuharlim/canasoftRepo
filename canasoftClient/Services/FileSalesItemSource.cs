using System.Globalization;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Services;
public class FileSalesItemItemSource : IItemSource<CreateSalesItemRequest>
{
    private readonly string _filePath;

    public FileSalesItemItemSource(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IEnumerable<CreateSalesItemRequest>> LoadAsync()
    {
        var lines = await File.ReadAllLinesAsync(_filePath);
        var salesItems = new List<CreateSalesItemRequest>();

        foreach (var line in lines.Skip(1)) 
        {
            var parts = line.Split(',');

            if (parts.Length < 11)
                    continue;

                if (!DateTime.TryParse(parts[0], out var salesDate))
                    continue;

                if (!decimal.TryParse(parts[9], NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity))
                    continue;

                if (!decimal.TryParse(parts[10], NumberStyles.Number, CultureInfo.InvariantCulture, out var subTotal))
                    continue;

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

        return salesItems;  
    }

}