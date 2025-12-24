namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;
using LotteryTracker.Core.Models;
using System.Collections.ObjectModel;

public partial class DashboardViewModel(
    IStatisticsService statisticsService,
    ITicketRepository ticketRepository,
    INavigationService navigationService) : BaseViewModel
{
    [ObservableProperty]
    private TicketStatistics? _overallStats;

    [ObservableProperty]
    private ObservableCollection<Ticket> _recentTickets = [];

    public override async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            OverallStats = await statisticsService.GetOverallStatisticsAsync();
            var tickets = await ticketRepository.GetAllTicketsAsync();
            RecentTickets = new ObservableCollection<Ticket>(tickets.Take(5));
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
    private void AddNewTicket() => navigationService.NavigateTo("AddTicket");

    [RelayCommand]
    private void ViewTicketDetail(Ticket ticket)
        => navigationService.NavigateTo("TicketDetail", ticket.Id);

    [RelayCommand]
    private void ViewAllTickets() => navigationService.NavigateTo("TicketHistory");

    [RelayCommand]
    private void ViewStatistics() => navigationService.NavigateTo("Statistics");
}
