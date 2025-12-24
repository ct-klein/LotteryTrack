namespace LotteryTracker.Infrastructure.Data;

using LotteryTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

public class LotteryTrackerDbContext : DbContext
{
    public LotteryTrackerDbContext(DbContextOptions<LotteryTrackerDbContext> options)
        : base(options) { }

    public DbSet<ScratchOffTicket> ScratchOffTickets => Set<ScratchOffTicket>();
    public DbSet<DrawGameTicket> DrawGameTickets => Set<DrawGameTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>()
            .HasDiscriminator<string>("TicketDiscriminator")
            .HasValue<ScratchOffTicket>("ScratchOff")
            .HasValue<DrawGameTicket>("DrawGame");

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Ticket>()
            .Property(t => t.PrizeAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.SerialNumber);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.PurchaseDate);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Status);

        modelBuilder.Entity<Ticket>()
            .Ignore(t => t.TicketType);
    }
}
