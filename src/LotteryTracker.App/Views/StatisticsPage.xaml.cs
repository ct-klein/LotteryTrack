namespace LotteryTracker.App.Views;

using LotteryTracker.App.ViewModels;
using LotteryTracker.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class StatisticsPage : Page
{
    public StatisticsViewModel ViewModel { get; }

    public StatisticsPage()
    {
        ViewModel = App.Services.GetRequiredService<StatisticsViewModel>();
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        UpdateLoadingState(true);
        await ViewModel.InitializeAsync();
        UpdateLoadingState(false);
        UpdateUI();
    }

    private void UpdateLoadingState(bool isLoading)
    {
        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        ContentArea.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateUI()
    {
        UpdateOverallStats();
        UpdateTypeStats();
        UpdateGameStats();
        UpdateMonthlyStats();
    }

    private void UpdateOverallStats()
    {
        var stats = ViewModel.OverallStats;
        if (stats != null)
        {
            TotalSpentText.Text = stats.TotalSpent.ToString("C2");
            TotalTicketsText.Text = stats.TotalTickets == 1 ? "1 ticket" : $"{stats.TotalTickets} tickets";
            TotalWonText.Text = stats.TotalWon.ToString("C2");
            WinnersText.Text = stats.WinningTickets == 1 ? "1 winner" : $"{stats.WinningTickets} winners";

            NetProfitText.Text = stats.NetProfit.ToString("C2");
            NetProfitText.Foreground = stats.NetProfit >= 0
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);

            WinRateText.Text = $"{stats.WinRate:F1}%";
            var resolved = stats.TotalTickets - stats.PendingTickets;
            WinRateSubtitleText.Text = $"{stats.WinningTickets} of {resolved} resolved";

            PendingCountText.Text = stats.PendingTickets.ToString();
            WinnerCountText.Text = stats.WinningTickets.ToString();
            LoserCountText.Text = stats.LosingTickets.ToString();
        }
        else
        {
            TotalSpentText.Text = "$0.00";
            TotalTicketsText.Text = "0 tickets";
            TotalWonText.Text = "$0.00";
            WinnersText.Text = "0 winners";
            NetProfitText.Text = "$0.00";
            WinRateText.Text = "0.0%";
            WinRateSubtitleText.Text = "0 of 0 resolved";
            PendingCountText.Text = "0";
            WinnerCountText.Text = "0";
            LoserCountText.Text = "0";
        }
    }

    private void UpdateTypeStats()
    {
        // Scratch-Off Stats
        var scratchOff = ViewModel.ScratchOffStats;
        if (scratchOff != null)
        {
            ScratchOffTotalText.Text = scratchOff.TotalTickets.ToString();
            ScratchOffWinRateText.Text = $"{scratchOff.WinRate:F1}%";
            ScratchOffSpentText.Text = scratchOff.TotalSpent.ToString("C2");
            ScratchOffWonText.Text = scratchOff.TotalWon.ToString("C2");
            ScratchOffNetText.Text = scratchOff.NetProfit.ToString("C2");
            ScratchOffNetText.Foreground = scratchOff.NetProfit >= 0
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        }
        else
        {
            ScratchOffTotalText.Text = "0";
            ScratchOffWinRateText.Text = "0.0%";
            ScratchOffSpentText.Text = "$0.00";
            ScratchOffWonText.Text = "$0.00";
            ScratchOffNetText.Text = "$0.00";
        }

        // Draw Game Stats
        var drawGame = ViewModel.DrawGameStats;
        if (drawGame != null)
        {
            DrawGameTotalText.Text = drawGame.TotalTickets.ToString();
            DrawGameWinRateText.Text = $"{drawGame.WinRate:F1}%";
            DrawGameSpentText.Text = drawGame.TotalSpent.ToString("C2");
            DrawGameWonText.Text = drawGame.TotalWon.ToString("C2");
            DrawGameNetText.Text = drawGame.NetProfit.ToString("C2");
            DrawGameNetText.Foreground = drawGame.NetProfit >= 0
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        }
        else
        {
            DrawGameTotalText.Text = "0";
            DrawGameWinRateText.Text = "0.0%";
            DrawGameSpentText.Text = "$0.00";
            DrawGameWonText.Text = "$0.00";
            DrawGameNetText.Text = "$0.00";
        }
    }

    private void UpdateGameStats()
    {
        if (ViewModel.GameStats.Count > 0)
        {
            GameStatsListView.ItemsSource = ViewModel.GameStats;
            GameStatsListView.Visibility = Visibility.Visible;
            NoGamesText.Visibility = Visibility.Collapsed;
        }
        else
        {
            GameStatsListView.Visibility = Visibility.Collapsed;
            NoGamesText.Visibility = Visibility.Visible;
        }
    }

    private void UpdateMonthlyStats()
    {
        if (ViewModel.MonthlyStats.Count > 0)
        {
            MonthlyStatsListView.ItemsSource = ViewModel.MonthlyStats;
            MonthlyStatsListView.Visibility = Visibility.Visible;
            NoMonthlyText.Visibility = Visibility.Collapsed;
        }
        else
        {
            MonthlyStatsListView.Visibility = Visibility.Collapsed;
            NoMonthlyText.Visibility = Visibility.Visible;
        }
    }

    private void GameStatsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue) return;

        if (args.Item is GameStatistics game && args.ItemContainer.ContentTemplateRoot is Grid grid)
        {
            var gameNameText = grid.FindName("GameNameText") as TextBlock;
            var gameTicketsText = grid.FindName("GameTicketsText") as TextBlock;
            var gameSpentText = grid.FindName("GameSpentText") as TextBlock;
            var gameWonText = grid.FindName("GameWonText") as TextBlock;
            var gameWinRateText = grid.FindName("GameWinRateText") as TextBlock;

            if (gameNameText != null)
                gameNameText.Text = game.GameName;

            if (gameTicketsText != null)
                gameTicketsText.Text = game.Statistics.TotalTickets.ToString();

            if (gameSpentText != null)
                gameSpentText.Text = game.Statistics.TotalSpent.ToString("C2");

            if (gameWonText != null)
                gameWonText.Text = game.Statistics.TotalWon.ToString("C2");

            if (gameWinRateText != null)
                gameWinRateText.Text = $"{game.Statistics.WinRate:F1}%";
        }
    }

    private void MonthlyStatsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue) return;

        if (args.Item is PeriodStatistics period && args.ItemContainer.ContentTemplateRoot is Grid grid)
        {
            var monthText = grid.FindName("MonthText") as TextBlock;
            var monthTicketsText = grid.FindName("MonthTicketsText") as TextBlock;
            var monthSpentText = grid.FindName("MonthSpentText") as TextBlock;
            var monthWonText = grid.FindName("MonthWonText") as TextBlock;
            var monthNetText = grid.FindName("MonthNetText") as TextBlock;

            if (monthText != null)
                monthText.Text = period.StartDate.ToString("MMM yyyy");

            if (monthTicketsText != null)
                monthTicketsText.Text = period.Statistics.TotalTickets.ToString();

            if (monthSpentText != null)
                monthSpentText.Text = period.Statistics.TotalSpent.ToString("C2");

            if (monthWonText != null)
                monthWonText.Text = period.Statistics.TotalWon.ToString("C2");

            if (monthNetText != null)
            {
                monthNetText.Text = period.Statistics.NetProfit.ToString("C2");
                monthNetText.Foreground = period.Statistics.NetProfit >= 0
                    ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                    : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }
    }
}
