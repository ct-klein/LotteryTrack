namespace LotteryTracker.Core.Models;

using LotteryTracker.Core.Entities;

public record TicketStatistics
{
    public int TotalTickets { get; init; }
    public int WinningTickets { get; init; }
    public int LosingTickets { get; init; }
    public int PendingTickets { get; init; }
    public decimal TotalSpent { get; init; }
    public decimal TotalWon { get; init; }
    public decimal NetProfit => TotalWon - TotalSpent;
    public double WinRate => TotalTickets - PendingTickets > 0
        ? (double)WinningTickets / (TotalTickets - PendingTickets) * 100
        : 0;
}

public record PeriodStatistics
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public required TicketStatistics Statistics { get; init; }
}

public record GameStatistics
{
    public required string GameName { get; init; }
    public TicketType TicketType { get; init; }
    public required TicketStatistics Statistics { get; init; }
}
