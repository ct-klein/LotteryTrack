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
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;

public partial class App : Application
{
    private Window? _window;
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LotteryTracker", "logs");

    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        // Configure Serilog before anything else
        ConfigureLogging();

        // Subscribe to unhandled exception events
        this.UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        this.InitializeComponent();
        Services = ConfigureServices();

        Log.Information("Application started");
    }

    private static void ConfigureLogging()
    {
        Directory.CreateDirectory(LogDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: Path.Combine(LogDirectory, "lottery-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled UI exception");
        e.Handled = true;
    }

    private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Unhandled AppDomain exception (IsTerminating: {IsTerminating})", e.IsTerminating);
        }
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging - register Serilog as the ILogger provider
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

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

        // Settings Service
        services.AddSingleton<ISettingsService, SettingsService>();

        // Navigation Service
        var navigationService = new NavigationService();
        navigationService.RegisterPage("Dashboard", typeof(DashboardPage));
        navigationService.RegisterPage("AddTicket", typeof(AddTicketPage));
        navigationService.RegisterPage("TicketHistory", typeof(TicketHistoryPage));
        navigationService.RegisterPage("TicketDetail", typeof(TicketDetailPage));
        navigationService.RegisterPage("Statistics", typeof(StatisticsPage));
        navigationService.RegisterPage("Settings", typeof(SettingsPage));
        services.AddSingleton<INavigationService>(navigationService);

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AddTicketViewModel>();
        services.AddTransient<TicketHistoryViewModel>();
        services.AddTransient<TicketDetailViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Barcode Service (registered after MainWindow is created, uses settings for camera selection)
        services.AddTransient<IBarcodeService>(sp =>
            new BarcodeService(MainWindow, sp.GetRequiredService<ISettingsService>()));

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        Log.Debug("OnLaunched starting");

        _window = new Window();
        MainWindow = _window;
        _window.Closed += Window_Closed;

        _window.Content = new ShellPage();
        _window.Title = "LotteryTracker";
        _window.Activate();

        // Ensure database is created
        Log.Debug("Initializing database");
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LotteryTrackerDbContext>();
        context.Database.EnsureCreated();
        Log.Information("Database initialized at {DbPath}", context.Database.GetDbConnection().DataSource);

        // Navigate to Dashboard
        var navigationService = Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo("Dashboard");
        Log.Debug("Navigation to Dashboard complete");
    }

    private static void Window_Closed(object sender, WindowEventArgs args)
    {
        Log.Information("Application shutting down");
        Log.CloseAndFlush();
    }
}
