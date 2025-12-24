namespace LotteryTracker.App.Views;

using LotteryTracker.App.ViewModels;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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
        await ViewModel.InitializeAsync();
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        var hasTickets = ViewModel.RecentTickets.Count > 0;
        EmptyStatePanel.Visibility = hasTickets ? Visibility.Collapsed : Visibility.Visible;
        TicketListView.Visibility = hasTickets ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TicketList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Ticket ticket)
        {
            ViewModel.ViewTicketDetailCommand.Execute(ticket);
        }
    }

    // Helper methods for x:Bind formatting (must be non-static for x:Bind)
    public string FormatCurrency(decimal amount) => amount.ToString("C2");

    public string FormatPercent(double value) => $"{value:F1}%";

    public string FormatTicketCount(int count) => count == 1 ? "1 ticket" : $"{count} tickets";

    public string FormatWinCount(int count) => count == 1 ? "1 winner" : $"{count} winners";
}
