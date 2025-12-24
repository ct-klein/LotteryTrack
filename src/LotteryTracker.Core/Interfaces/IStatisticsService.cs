namespace LotteryTracker.Core.Interfaces;

using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Models;

public interface IStatisticsService
{
    Task<TicketStatistics> GetOverallStatisticsAsync();
    Task<TicketStatistics> GetStatisticsByTypeAsync(TicketType type);
    Task<PeriodStatistics> GetStatisticsByPeriodAsync(DateTime start, DateTime end);
    Task<IEnumerable<GameStatistics>> GetStatisticsByGameAsync();
    Task<IEnumerable<PeriodStatistics>> GetMonthlyStatisticsAsync(int months = 12);
}
