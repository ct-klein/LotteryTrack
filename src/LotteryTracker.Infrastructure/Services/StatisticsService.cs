namespace LotteryTracker.Infrastructure.Services;

using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;
using LotteryTracker.Core.Models;

public class StatisticsService(ITicketRepository ticketRepository) : IStatisticsService
{
    public async Task<TicketStatistics> GetOverallStatisticsAsync()
    {
        var tickets = await ticketRepository.GetAllTicketsAsync();
        return CalculateStatistics(tickets);
    }

    public async Task<TicketStatistics> GetStatisticsByTypeAsync(TicketType type)
    {
        IEnumerable<Ticket> tickets = type switch
        {
            TicketType.ScratchOff => await ticketRepository.GetScratchOffTicketsAsync(),
            TicketType.DrawGame => await ticketRepository.GetDrawGameTicketsAsync(),
            _ => []
        };

        return CalculateStatistics(tickets);
    }

    public async Task<PeriodStatistics> GetStatisticsByPeriodAsync(DateTime start, DateTime end)
    {
        var tickets = await ticketRepository.GetTicketsByDateRangeAsync(start, end);
        return new PeriodStatistics
        {
            StartDate = start,
            EndDate = end,
            Statistics = CalculateStatistics(tickets)
        };
    }

    public async Task<IEnumerable<GameStatistics>> GetStatisticsByGameAsync()
    {
        var scratchOffs = await ticketRepository.GetScratchOffTicketsAsync();
        var drawGames = await ticketRepository.GetDrawGameTicketsAsync();

        var scratchOffStats = scratchOffs
            .GroupBy(t => t.GameName)
            .Select(g => new GameStatistics
            {
                GameName = g.Key,
                TicketType = TicketType.ScratchOff,
                Statistics = CalculateStatistics(g.Cast<Ticket>())
            });

        var drawGameStats = drawGames
            .GroupBy(t => t.GameType == DrawGameType.Other ? t.CustomGameName ?? "Other" : t.GameType.ToString())
            .Select(g => new GameStatistics
            {
                GameName = g.Key,
                TicketType = TicketType.DrawGame,
                Statistics = CalculateStatistics(g.Cast<Ticket>())
            });

        return scratchOffStats.Concat(drawGameStats).OrderByDescending(s => s.Statistics.TotalTickets);
    }

    public async Task<IEnumerable<PeriodStatistics>> GetMonthlyStatisticsAsync(int months = 12)
    {
        var results = new List<PeriodStatistics>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < months; i++)
        {
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var end = start.AddMonths(1).AddDays(-1);

            var tickets = await ticketRepository.GetTicketsByDateRangeAsync(start, end);
            results.Add(new PeriodStatistics
            {
                StartDate = start,
                EndDate = end,
                Statistics = CalculateStatistics(tickets)
            });
        }

        return results.OrderBy(p => p.StartDate);
    }

    private static TicketStatistics CalculateStatistics(IEnumerable<Ticket> tickets)
    {
        var ticketList = tickets.ToList();
        return new TicketStatistics
        {
            TotalTickets = ticketList.Count,
            WinningTickets = ticketList.Count(t => t.Status is TicketStatus.Winner or TicketStatus.Claimed),
            LosingTickets = ticketList.Count(t => t.Status == TicketStatus.Loser),
            PendingTickets = ticketList.Count(t => t.Status == TicketStatus.Pending),
            TotalSpent = ticketList.Sum(t => t.Price),
            TotalWon = ticketList.Where(t => t.PrizeAmount.HasValue).Sum(t => t.PrizeAmount!.Value)
        };
    }
}
