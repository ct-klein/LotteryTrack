namespace LotteryTracker.Infrastructure.Repositories;

using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;
using LotteryTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class TicketRepository(LotteryTrackerDbContext context) : ITicketRepository
{
    public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
    {
        var scratchOffs = await context.ScratchOffTickets.ToListAsync();
        var drawGames = await context.DrawGameTickets.ToListAsync();
        return scratchOffs.Cast<Ticket>()
            .Concat(drawGames)
            .OrderByDescending(t => t.PurchaseDate);
    }

    public async Task<IEnumerable<ScratchOffTicket>> GetScratchOffTicketsAsync()
    {
        return await context.ScratchOffTickets
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DrawGameTicket>> GetDrawGameTicketsAsync()
    {
        return await context.DrawGameTickets
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        var scratchOff = await context.ScratchOffTickets.FindAsync(id);
        if (scratchOff != null) return scratchOff;

        return await context.DrawGameTickets.FindAsync(id);
    }

    public async Task<Ticket?> GetTicketBySerialNumberAsync(string serialNumber)
    {
        var scratchOff = await context.ScratchOffTickets
            .FirstOrDefaultAsync(t => t.SerialNumber == serialNumber);
        if (scratchOff != null) return scratchOff;

        return await context.DrawGameTickets
            .FirstOrDefaultAsync(t => t.SerialNumber == serialNumber);
    }

    public async Task<Ticket> AddTicketAsync(Ticket ticket)
    {
        ticket.CreatedAt = DateTime.UtcNow;

        if (ticket is ScratchOffTicket scratchOff)
        {
            context.ScratchOffTickets.Add(scratchOff);
        }
        else if (ticket is DrawGameTicket drawGame)
        {
            context.DrawGameTickets.Add(drawGame);
        }

        await context.SaveChangesAsync();
        return ticket;
    }

    public async Task UpdateTicketAsync(Ticket ticket)
    {
        ticket.ModifiedAt = DateTime.UtcNow;

        if (ticket is ScratchOffTicket scratchOff)
        {
            context.ScratchOffTickets.Update(scratchOff);
        }
        else if (ticket is DrawGameTicket drawGame)
        {
            context.DrawGameTickets.Update(drawGame);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteTicketAsync(int id)
    {
        var ticket = await GetTicketByIdAsync(id);
        if (ticket == null) return;

        if (ticket is ScratchOffTicket scratchOff)
        {
            context.ScratchOffTickets.Remove(scratchOff);
        }
        else if (ticket is DrawGameTicket drawGame)
        {
            context.DrawGameTickets.Remove(drawGame);
        }

        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByDateRangeAsync(DateTime start, DateTime end)
    {
        var scratchOffs = await context.ScratchOffTickets
            .Where(t => t.PurchaseDate >= start && t.PurchaseDate <= end)
            .ToListAsync();

        var drawGames = await context.DrawGameTickets
            .Where(t => t.PurchaseDate >= start && t.PurchaseDate <= end)
            .ToListAsync();

        return scratchOffs.Cast<Ticket>()
            .Concat(drawGames)
            .OrderByDescending(t => t.PurchaseDate);
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status)
    {
        var scratchOffs = await context.ScratchOffTickets
            .Where(t => t.Status == status)
            .ToListAsync();

        var drawGames = await context.DrawGameTickets
            .Where(t => t.Status == status)
            .ToListAsync();

        return scratchOffs.Cast<Ticket>()
            .Concat(drawGames)
            .OrderByDescending(t => t.PurchaseDate);
    }
}
