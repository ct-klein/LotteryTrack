namespace LotteryTracker.Core.Entities;

public abstract class Ticket : BaseEntity
{
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Price { get; set; }
    public string? StoreName { get; set; }
    public string? StoreLocation { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Pending;
    public decimal? PrizeAmount { get; set; }
    public string? Notes { get; set; }
    public abstract TicketType TicketType { get; }
}
