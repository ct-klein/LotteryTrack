namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;
using LotteryTracker.Core.Models;
using System.Collections.ObjectModel;

public partial class StatisticsViewModel(IStatisticsService statisticsService) : BaseViewModel
{
    [ObservableProperty]
    private TicketStatistics? _overallStats;

    [ObservableProperty]
    private TicketStatistics? _scratchOffStats;

    [ObservableProperty]
    private TicketStatistics? _drawGameStats;

    [ObservableProperty]
    private ObservableCollection<PeriodStatistics> _monthlyStats = [];

    [ObservableProperty]
    private ObservableCollection<GameStatistics> _gameStats = [];

    public override async Task InitializeAsync()
    {
        await LoadStatisticsAsync();
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        IsLoading = true;
        try
        {
            OverallStats = await statisticsService.GetOverallStatisticsAsync();
            ScratchOffStats = await statisticsService.GetStatisticsByTypeAsync(TicketType.ScratchOff);
            DrawGameStats = await statisticsService.GetStatisticsByTypeAsync(TicketType.DrawGame);

            var monthly = await statisticsService.GetMonthlyStatisticsAsync(12);
            MonthlyStats = new ObservableCollection<PeriodStatistics>(monthly);

            var games = await statisticsService.GetStatisticsByGameAsync();
            GameStats = new ObservableCollection<GameStatistics>(games);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
