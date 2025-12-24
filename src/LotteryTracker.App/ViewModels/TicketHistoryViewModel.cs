namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;
using System.Collections.ObjectModel;

public partial class TicketHistoryViewModel(
    ITicketRepository ticketRepository,
    INavigationService navigationService) : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Ticket> _tickets = [];

    [ObservableProperty]
    private TicketType? _filterTicketType;

    [ObservableProperty]
    private TicketStatus? _filterStatus;

    [ObservableProperty]
    private DateTime? _filterStartDate;

    [ObservableProperty]
    private DateTime? _filterEndDate;

    [ObservableProperty]
    private string? _searchQuery;

    public override async Task InitializeAsync()
    {
        await LoadTicketsAsync();
    }

    [RelayCommand]
    private async Task LoadTicketsAsync()
    {
        IsLoading = true;
        try
        {
            var allTickets = await ticketRepository.GetAllTicketsAsync();
            Tickets = new ObservableCollection<Ticket>(allTickets);
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
    private async Task ApplyFiltersAsync()
    {
        IsLoading = true;
        try
        {
            IEnumerable<Ticket> tickets;

            if (FilterStartDate.HasValue && FilterEndDate.HasValue)
            {
                tickets = await ticketRepository.GetTicketsByDateRangeAsync(
                    FilterStartDate.Value, FilterEndDate.Value);
            }
            else if (FilterStatus.HasValue)
            {
                tickets = await ticketRepository.GetTicketsByStatusAsync(FilterStatus.Value);
            }
            else
            {
                tickets = await ticketRepository.GetAllTicketsAsync();
            }

            if (FilterTicketType.HasValue)
            {
                tickets = tickets.Where(t => t.TicketType == FilterTicketType.Value);
            }

            Tickets = new ObservableCollection<Ticket>(tickets);
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
    private async Task ClearFiltersAsync()
    {
        FilterTicketType = null;
        FilterStatus = null;
        FilterStartDate = null;
        FilterEndDate = null;
        SearchQuery = null;
        await LoadTicketsAsync();
    }

    [RelayCommand]
    private void ViewTicketDetail(Ticket ticket)
        => navigationService.NavigateTo("TicketDetail", ticket.Id);

    [RelayCommand]
    private void AddNewTicket() => navigationService.NavigateTo("AddTicket");
}
