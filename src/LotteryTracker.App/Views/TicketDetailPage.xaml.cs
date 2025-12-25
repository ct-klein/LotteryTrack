namespace LotteryTracker.App.Views;

using LotteryTracker.App.ViewModels;
using LotteryTracker.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class TicketDetailPage : Page
{
    public TicketDetailViewModel ViewModel { get; }

    public TicketDetailPage()
    {
        ViewModel = App.Services.GetRequiredService<TicketDetailViewModel>();
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        if (e.Parameter is int ticketId)
        {
            LoadingRing.IsActive = true;
            ContentArea.Visibility = Visibility.Collapsed;
            await ViewModel.LoadTicketAsync(ticketId);
            LoadingRing.IsActive = false;
            ContentArea.Visibility = Visibility.Visible;
            UpdateUI();
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ErrorMessage))
        {
            ErrorBar.Message = ViewModel.ErrorMessage ?? "";
            ErrorBar.IsOpen = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GoBackCommand.Execute(null);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteTicketCommand.Execute(null);
    }

    private void MarkLoserButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.MarkAsLoserCommand.Execute(null);
        UpdateUI();
    }

    private void MarkWinnerButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.MarkAsWinnerCommand.Execute(null);
        UpdateUI();
    }

    private void ClaimButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.MarkAsClaimedCommand.Execute(null);
        UpdateUI();
    }

    private void PrizeAmountInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.PrizeAmountValue = double.IsNaN(args.NewValue) ? 0 : args.NewValue;
    }

    private void UpdateUI()
    {
        if (ViewModel.Ticket == null) return;

        var ticket = ViewModel.Ticket;

        // Status and prize
        StatusText.Text = ticket.Status switch
        {
            TicketStatus.Pending => "Pending",
            TicketStatus.Winner => "Winner",
            TicketStatus.Loser => "No Win",
            TicketStatus.Claimed => "Claimed",
            _ => "Unknown"
        };
        StatusBadge.Background = ticket.Status switch
        {
            TicketStatus.Pending => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
            TicketStatus.Winner => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
            TicketStatus.Loser => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
            TicketStatus.Claimed => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
            _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
        };

        // Prize display
        if (ticket.PrizeAmount.HasValue && ticket.PrizeAmount > 0)
        {
            PrizeDisplay.Text = ticket.PrizeAmount.Value.ToString("C");
            PrizeDisplay.Visibility = Visibility.Visible;
        }
        else
        {
            PrizeDisplay.Visibility = Visibility.Collapsed;
        }

        // Common fields
        PurchaseDateDisplay.Text = ticket.PurchaseDate.ToString("MMM d, yyyy");
        PriceDisplay.Text = ticket.Price.ToString("C");
        SerialNumberDisplay.Text = ticket.SerialNumber ?? "N/A";
        StoreDisplay.Text = ticket.StoreName ?? "N/A";
        StoreLocationDisplay.Text = ticket.StoreLocation ?? "N/A";

        // Notes section
        if (!string.IsNullOrWhiteSpace(ticket.Notes))
        {
            NotesSection.Visibility = Visibility.Visible;
            NotesDisplay.Text = ticket.Notes;
        }

        // Claim button only visible for winners
        ClaimButton.Visibility = ticket.Status == TicketStatus.Winner
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Type-specific fields
        switch (ticket)
        {
            case ScratchOffTicket sot:
                TicketTypeIcon.Glyph = "\uE8C6";
                TicketNameDisplay.Text = sot.GameName;
                TicketTypeDisplay.Text = "Scratch-Off Ticket";

                ScratchOffInfoSection.Visibility = Visibility.Visible;
                DrawGameInfoSection.Visibility = Visibility.Collapsed;

                GameNumberDisplay.Text = sot.GameNumber ?? "N/A";
                TicketNumberDisplay.Text = sot.TicketNumber?.ToString() ?? "N/A";
                break;

            case DrawGameTicket dgt:
                TicketTypeIcon.Glyph = "\uE81C";
                TicketNameDisplay.Text = dgt.GameType == DrawGameType.Other
                    ? dgt.CustomGameName ?? "Draw Game"
                    : dgt.GameType.ToString();
                TicketTypeDisplay.Text = "Draw Game Ticket";

                ScratchOffInfoSection.Visibility = Visibility.Collapsed;
                DrawGameInfoSection.Visibility = Visibility.Visible;

                DrawDateDisplay.Text = dgt.DrawDate.ToString("MMM d, yyyy");
                QuickPickDisplay.Text = dgt.IsQuickPick ? "Quick Pick" : "Manual Selection";
                NumbersSelectedDisplay.Text = dgt.NumbersSelected;
                BonusNumbersDisplay.Text = dgt.BonusNumbers ?? "N/A";
                NumberOfDrawsDisplay.Text = dgt.NumberOfDraws.ToString();
                break;
        }
    }
}
