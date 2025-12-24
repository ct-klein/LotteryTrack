namespace LotteryTracker.App.Views;

using LotteryTracker.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class ShellPage : Page
{
    private readonly NavigationService _navigationService;

    public ShellPage()
    {
        this.InitializeComponent();

        _navigationService = (NavigationService)App.Services.GetRequiredService<INavigationService>();
        _navigationService.Frame = ContentFrame;

        ContentFrame.Navigated += ContentFrame_Navigated;
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            // Navigate to settings if implemented
            return;
        }

        if (args.SelectedItem is NavigationViewItem item && item.Tag is string pageKey)
        {
            _navigationService.NavigateTo(pageKey);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        GoBack();
    }

    private void OnEscapePressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (GoBack())
        {
            args.Handled = true;
        }
    }

    private bool GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
            return true;
        }
        return false;
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        NavView.IsBackEnabled = _navigationService.CanGoBack;

        // Update selected item based on current page
        var pageType = ContentFrame.CurrentSourcePageType;
        foreach (var item in NavView.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag is string tag)
            {
                var registeredType = GetPageType(tag);
                if (registeredType == pageType)
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }
    }

    private static Type? GetPageType(string pageKey)
    {
        return pageKey switch
        {
            "Dashboard" => typeof(DashboardPage),
            "AddTicket" => typeof(AddTicketPage),
            "TicketHistory" => typeof(TicketHistoryPage),
            "TicketDetail" => typeof(TicketDetailPage),
            "Statistics" => typeof(StatisticsPage),
            _ => null
        };
    }
}
