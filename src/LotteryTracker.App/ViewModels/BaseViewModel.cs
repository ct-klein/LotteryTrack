namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public virtual Task InitializeAsync() => Task.CompletedTask;
}
