namespace LotteryTracker.App.Views;

using LotteryTracker.App.ViewModels;
using LotteryTracker.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        UpdateLoadingState(true);
        await ViewModel.InitializeAsync();
        TicketListView.ItemsSource = ViewModel.RecentTickets;
        UpdateLoadingState(false);
        UpdateEmptyState();
        UpdateStats();
    }

    private void UpdateLoadingState(bool isLoading)
    {
        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        MainContent.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateEmptyState()
    {
        var hasTickets = ViewModel.RecentTickets.Count > 0;
        EmptyStatePanel.Visibility = hasTickets ? Visibility.Collapsed : Visibility.Visible;
        TicketListView.Visibility = hasTickets ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateStats()
    {
        var stats = ViewModel.OverallStats;
        if (stats != null)
        {
            TotalSpentText.Text = stats.TotalSpent.ToString("C2");
            TotalTicketsText.Text = stats.TotalTickets == 1 ? "1 ticket" : $"{stats.TotalTickets} tickets";
            TotalWonText.Text = stats.TotalWon.ToString("C2");
            NetProfitText.Text = stats.NetProfit.ToString("C2");
            WinRateText.Text = $"{stats.WinRate:F1}%";
            WinCountText.Text = stats.WinningTickets == 1 ? "1 winner" : $"{stats.WinningTickets} winners";
            PendingCountText.Text = stats.PendingTickets.ToString();
            WinnersCountText.Text = stats.WinningTickets.ToString();
            LosersCountText.Text = stats.LosingTickets.ToString();
        }
        else
        {
            TotalSpentText.Text = "$0.00";
            TotalTicketsText.Text = "0 tickets";
            TotalWonText.Text = "$0.00";
            NetProfitText.Text = "$0.00";
            WinRateText.Text = "0.0%";
            WinCountText.Text = "0 winners";
            PendingCountText.Text = "0";
            WinnersCountText.Text = "0";
            LosersCountText.Text = "0";
        }
    }

    private void AddTicketButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddNewTicketCommand.Execute(null);
    }

    private void ViewAllButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ViewAllTicketsCommand.Execute(null);
    }

    private void ViewStatisticsButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ViewStatisticsCommand.Execute(null);
    }

    private void TicketList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Ticket ticket)
        {
            ViewModel.ViewTicketDetailCommand.Execute(ticket);
        }
    }

    private void TicketListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            return;
        }

        if (args.Item is Ticket ticket && args.ItemContainer.ContentTemplateRoot is Grid grid)
        {
            var statusIndicator = grid.FindName("StatusIndicator") as Ellipse;
            var purchaseDateText = grid.FindName("PurchaseDateText") as TextBlock;
            var storeNameText = grid.FindName("StoreNameText") as TextBlock;
            var statusText = grid.FindName("StatusText") as TextBlock;
            var priceText = grid.FindName("PriceText") as TextBlock;

            // Set status indicator color
            if (statusIndicator != null)
            {
                statusIndicator.Fill = ticket.Status switch
                {
                    TicketStatus.Pending => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                    TicketStatus.Winner => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                    TicketStatus.Loser => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    TicketStatus.Claimed => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                    _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
            }

            // Set purchase date
            if (purchaseDateText != null)
            {
                purchaseDateText.Text = ticket.PurchaseDate.ToString("MMM dd, yyyy");
            }

            // Set store name
            if (storeNameText != null)
            {
                storeNameText.Text = ticket.StoreName ?? "";
            }

            // Set status text
            if (statusText != null)
            {
                statusText.Text = ticket.Status switch
                {
                    TicketStatus.Pending => "Pending",
                    TicketStatus.Winner => "Winner",
                    TicketStatus.Loser => "No Win",
                    TicketStatus.Claimed => "Claimed",
                    _ => "Unknown"
                };
                statusText.Foreground = ticket.Status switch
                {
                    TicketStatus.Pending => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                    TicketStatus.Winner => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                    TicketStatus.Loser => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    TicketStatus.Claimed => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                    _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
            }

            // Set price
            if (priceText != null)
            {
                priceText.Text = ticket.Price.ToString("C");
            }
        }
    }
}
