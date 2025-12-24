namespace LotteryTracker.App.Services;

using Microsoft.UI.Xaml.Controls;

public class NavigationService : INavigationService
{
    private Frame? _frame;
    private readonly Dictionary<string, Type> _pages = [];

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void RegisterPage(string key, Type pageType)
    {
        _pages[key] = pageType;
    }

    public void NavigateTo(string pageKey, object? parameter = null)
    {
        if (_frame == null) return;

        if (_pages.TryGetValue(pageKey, out var pageType))
        {
            _frame.Navigate(pageType, parameter);
        }
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }
}
