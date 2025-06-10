namespace CanasoftClient.Contracts.Request;

public class CreateSalesItemRequest
{
    public string CompanyCode { get; set; } = default!;
    public DateTime SalesDate { get; set; }
    public string ItemId { get; set; } = default!;
    public string ItemName { get; set; } = default!;
    public string? ItemCode { get; set; }
    public string GroupItemId { get; set; } = default!;
    public string GroupItemName { get; set; } = default!;
    public string WarehouseId { get; set; } = default!;
    public string WarehouseName { get; set; } = default!;
    public string SalesName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal SubTotal { get; set; }
}