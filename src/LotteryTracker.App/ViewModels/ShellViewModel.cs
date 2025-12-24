namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;

public partial class ShellViewModel(INavigationService navigationService) : BaseViewModel
{
    [ObservableProperty]
    private object? _selectedMenuItem;

    [RelayCommand]
    private void NavigateTo(string pageKey)
    {
        navigationService.NavigateTo(pageKey);
    }
}
