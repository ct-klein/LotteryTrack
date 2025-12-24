namespace LotteryTracker.Core.Entities;

public class ScratchOffTicket : Ticket
{
    public string GameName { get; set; } = string.Empty;
    public string? GameNumber { get; set; }
    public int? TicketNumber { get; set; }
    public override TicketType TicketType => TicketType.ScratchOff;
}
