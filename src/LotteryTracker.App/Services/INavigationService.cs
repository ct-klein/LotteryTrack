namespace LotteryTracker.App.Services;

public interface INavigationService
{
    void NavigateTo(string pageKey, object? parameter = null);
    void GoBack();
    bool CanGoBack { get; }
}
