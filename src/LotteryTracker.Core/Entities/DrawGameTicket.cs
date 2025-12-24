namespace LotteryTracker.Core.Entities;

public class DrawGameTicket : Ticket
{
    public DrawGameType GameType { get; set; }
    public string? CustomGameName { get; set; }
    public string NumbersSelected { get; set; } = string.Empty;
    public string? BonusNumbers { get; set; }
    public DateTime DrawDate { get; set; }
    public bool IsQuickPick { get; set; }
    public int NumberOfDraws { get; set; } = 1;
    public override TicketType TicketType => TicketType.DrawGame;
}
