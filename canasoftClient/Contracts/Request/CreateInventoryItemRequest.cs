namespace CanasoftClient.Contracts.Request;

public class CreateInventoryItemRequest
{
    public string CompanyCode { get; set; } = default!;
    public string ItemId { get; set; } = default!;
    public string WarehouseId { get; set; } = default!;
    public string ItemName { get; set; } = default!;
    public string WarehouseName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string GroupItemId { get; set; } = default!;
    public string GroupItemName { get; set; } = default!;
    public string? ItemCode { get; set; }
}
