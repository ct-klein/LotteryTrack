namespace LotteryTracker.App.Views;

using LotteryTracker.App.ViewModels;
using LotteryTracker.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class TicketHistoryPage : Page
{
    public TicketHistoryViewModel ViewModel { get; }

    public TicketHistoryPage()
    {
        ViewModel = App.Services.GetRequiredService<TicketHistoryViewModel>();
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        TicketListView.ItemsSource = ViewModel.Tickets;
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        var hasTickets = ViewModel.Tickets.Count > 0;
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

    private async void TicketTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TicketTypeFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            ViewModel.FilterTicketType = tag switch
            {
                "ScratchOff" => TicketType.ScratchOff,
                "DrawGame" => TicketType.DrawGame,
                _ => null
            };
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            UpdateEmptyState();
        }
    }

    private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StatusFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            ViewModel.FilterStatus = tag switch
            {
                "Pending" => TicketStatus.Pending,
                "Winner" => TicketStatus.Winner,
                "Loser" => TicketStatus.Loser,
                "Claimed" => TicketStatus.Claimed,
                _ => null
            };
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            UpdateEmptyState();
        }
    }

    private async void DateFilter_Changed(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        ViewModel.FilterStartDate = StartDatePicker.Date?.DateTime;
        ViewModel.FilterEndDate = EndDatePicker.Date?.DateTime;
        await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        UpdateEmptyState();
    }

    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        TicketTypeFilter.SelectedIndex = 0;
        StatusFilter.SelectedIndex = 0;
        StartDatePicker.Date = null;
        EndDatePicker.Date = null;
        UpdateEmptyState();
    }

    private void TicketListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            return;
        }

        if (args.Item is Ticket ticket && args.ItemContainer.ContentTemplateRoot is Grid grid)
        {
            // Find named elements in the grid
            var ticketIcon = grid.FindName("TicketIcon") as FontIcon;
            var ticketNameText = grid.FindName("TicketNameText") as TextBlock;
            var purchaseDateText = grid.FindName("PurchaseDateText") as TextBlock;
            var storeNameText = grid.FindName("StoreNameText") as TextBlock;
            var storeSeparator = grid.FindName("StoreSeparator") as TextBlock;
            var priceText = grid.FindName("PriceText") as TextBlock;
            var statusBadge = grid.FindName("StatusBadge") as Border;
            var statusText = grid.FindName("StatusText") as TextBlock;
            var prizeText = grid.FindName("PrizeText") as TextBlock;

            // Set ticket icon
            if (ticketIcon != null)
            {
                ticketIcon.Glyph = ticket.TicketType switch
                {
                    TicketType.ScratchOff => "\uE8C6",
                    TicketType.DrawGame => "\uE81C",
                    _ => "\uE8C6"
                };
            }

            // Set ticket name
            if (ticketNameText != null)
            {
                ticketNameText.Text = ticket switch
                {
                    ScratchOffTicket sot => sot.GameName,
                    DrawGameTicket dgt => dgt.GameType == DrawGameType.Other
                        ? dgt.CustomGameName ?? "Draw Game"
                        : dgt.GameType.ToString(),
                    _ => "Unknown"
                };
            }

            // Set purchase date
            if (purchaseDateText != null)
            {
                purchaseDateText.Text = ticket.PurchaseDate.ToString("MMM d, yyyy");
            }

            // Set store name
            if (storeNameText != null && storeSeparator != null)
            {
                if (!string.IsNullOrWhiteSpace(ticket.StoreName))
                {
                    storeNameText.Text = ticket.StoreName;
                    storeNameText.Visibility = Visibility.Visible;
                    storeSeparator.Visibility = Visibility.Visible;
                }
                else
                {
                    storeNameText.Visibility = Visibility.Collapsed;
                    storeSeparator.Visibility = Visibility.Collapsed;
                }
            }

            // Set price
            if (priceText != null)
            {
                priceText.Text = ticket.Price.ToString("C");
            }

            // Set status badge
            if (statusBadge != null && statusText != null)
            {
                statusText.Text = ticket.Status switch
                {
                    TicketStatus.Pending => "Pending",
                    TicketStatus.Winner => "Winner",
                    TicketStatus.Loser => "No Win",
                    TicketStatus.Claimed => "Claimed",
                    _ => "Unknown"
                };

                statusBadge.Background = ticket.Status switch
                {
                    TicketStatus.Pending => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                    TicketStatus.Winner => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                    TicketStatus.Loser => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    TicketStatus.Claimed => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                    _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
            }

            // Set prize amount
            if (prizeText != null)
            {
                if (ticket.PrizeAmount.HasValue && ticket.PrizeAmount.Value > 0)
                {
                    prizeText.Text = ticket.PrizeAmount.Value.ToString("C");
                    prizeText.Visibility = Visibility.Visible;
                }
                else
                {
                    prizeText.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
