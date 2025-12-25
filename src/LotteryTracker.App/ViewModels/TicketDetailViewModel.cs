namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;

public partial class TicketDetailViewModel(
    ITicketRepository ticketRepository,
    INavigationService navigationService) : BaseViewModel
{
    [ObservableProperty]
    private Ticket? _ticket;

    [ObservableProperty]
    private decimal? _prizeAmount;

    [ObservableProperty]
    private double _prizeAmountValue;

    public async Task LoadTicketAsync(int ticketId)
    {
        IsLoading = true;
        try
        {
            Ticket = await ticketRepository.GetTicketByIdAsync(ticketId);
            if (Ticket != null)
            {
                PrizeAmount = Ticket.PrizeAmount;
                PrizeAmountValue = (double)(Ticket.PrizeAmount ?? 0);
            }
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

    [RelayCommand]
    private async Task MarkAsWinnerAsync()
    {
        if (Ticket == null) return;

        try
        {
            Ticket.Status = TicketStatus.Winner;
            Ticket.PrizeAmount = (decimal)PrizeAmountValue;
            await ticketRepository.UpdateTicketAsync(Ticket);
            OnPropertyChanged(nameof(Ticket));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task MarkAsLoserAsync()
    {
        if (Ticket == null) return;

        try
        {
            Ticket.Status = TicketStatus.Loser;
            Ticket.PrizeAmount = null;
            await ticketRepository.UpdateTicketAsync(Ticket);
            OnPropertyChanged(nameof(Ticket));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task MarkAsClaimedAsync()
    {
        if (Ticket == null || Ticket.Status != TicketStatus.Winner) return;

        try
        {
            Ticket.Status = TicketStatus.Claimed;
            await ticketRepository.UpdateTicketAsync(Ticket);
            OnPropertyChanged(nameof(Ticket));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteTicketAsync()
    {
        if (Ticket == null) return;

        try
        {
            await ticketRepository.DeleteTicketAsync(Ticket.Id);
            navigationService.GoBack();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void GoBack() => navigationService.GoBack();
}
