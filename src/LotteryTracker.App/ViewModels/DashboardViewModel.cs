namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;
using LotteryTracker.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

public partial class DashboardViewModel(
    IStatisticsService statisticsService,
    ITicketRepository ticketRepository,
    INavigationService navigationService,
    ILogger<DashboardViewModel> logger) : BaseViewModel
{
    [ObservableProperty]
    private TicketStatistics? _overallStats;

    [ObservableProperty]
    private ObservableCollection<Ticket> _recentTickets = [];

    public override async Task InitializeAsync()
    {
        logger.LogDebug("Initializing dashboard");
        IsLoading = true;
        try
        {
            OverallStats = await statisticsService.GetOverallStatisticsAsync();
            var tickets = await ticketRepository.GetAllTicketsAsync();
            RecentTickets = new ObservableCollection<Ticket>(tickets.Take(5));
            logger.LogInformation("Dashboard loaded with {TicketCount} recent tickets", RecentTickets.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load dashboard data");
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
