namespace LotteryTracker.App;

using LotteryTracker.App.Services;
using LotteryTracker.App.ViewModels;
using LotteryTracker.App.Views;
using LotteryTracker.Core.Interfaces;
using LotteryTracker.Infrastructure.Data;
using LotteryTracker.Infrastructure.Repositories;
using LotteryTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

public partial class App : Application
{
    private Window? _window;

    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Database
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LotteryTracker", "lottery.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<LotteryTrackerDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Repositories & Services
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // Navigation Service
        var navigationService = new NavigationService();
        navigationService.RegisterPage("Dashboard", typeof(DashboardPage));
        navigationService.RegisterPage("AddTicket", typeof(AddTicketPage));
        navigationService.RegisterPage("TicketHistory", typeof(TicketHistoryPage));
        navigationService.RegisterPage("TicketDetail", typeof(TicketDetailPage));
        navigationService.RegisterPage("Statistics", typeof(StatisticsPage));
        services.AddSingleton<INavigationService>(navigationService);

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AddTicketViewModel>();
        services.AddTransient<TicketHistoryViewModel>();
        services.AddTransient<TicketDetailViewModel>();
        services.AddTransient<StatisticsViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        _window = new Window();
        MainWindow = _window;

        _window.Content = new ShellPage();
        _window.Title = "LotteryTracker";
        _window.Activate();

        // Ensure database is created
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LotteryTrackerDbContext>();
        context.Database.EnsureCreated();

        // Navigate to Dashboard
        var navigationService = Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo("Dashboard");
    }
}
