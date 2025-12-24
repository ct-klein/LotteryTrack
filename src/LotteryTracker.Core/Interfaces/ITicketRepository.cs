namespace LotteryTracker.Core.Interfaces;

using LotteryTracker.Core.Entities;

public interface ITicketRepository
{
    Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    Task<IEnumerable<ScratchOffTicket>> GetScratchOffTicketsAsync();
    Task<IEnumerable<DrawGameTicket>> GetDrawGameTicketsAsync();
    Task<Ticket?> GetTicketByIdAsync(int id);
    Task<Ticket?> GetTicketBySerialNumberAsync(string serialNumber);
    Task<Ticket> AddTicketAsync(Ticket ticket);
    Task UpdateTicketAsync(Ticket ticket);
    Task DeleteTicketAsync(int id);
    Task<IEnumerable<Ticket>> GetTicketsByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status);
}
