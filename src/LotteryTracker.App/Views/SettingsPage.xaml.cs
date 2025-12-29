namespace LotteryTracker.App.Views;

using LotteryTracker.App.Services;
using LotteryTracker.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();

        BackButton.Click += BackButton_Click;
        RefreshButton.Click += RefreshButton_Click;
        CameraComboBox.SelectionChanged += CameraComboBox_SelectionChanged;

        Loaded += SettingsPage_Loaded;
    }

    private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCamerasAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadCamerasAsync();
    }

    private async Task LoadCamerasAsync()
    {
        UpdateLoadingState(true);

        await ViewModel.LoadCamerasAsync();

        UpdateLoadingState(false);
        UpdateUI();
    }

    private void UpdateLoadingState(bool isLoading)
    {
        LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        CameraComboBox.IsEnabled = !isLoading;
        RefreshButton.IsEnabled = !isLoading;
    }

    private void UpdateUI()
    {
        // Update camera combo box
        CameraComboBox.ItemsSource = ViewModel.AvailableCameras;
        CameraComboBox.SelectedItem = ViewModel.SelectedCamera;

        // Update status
        NoCamerasInfoBar.IsOpen = ViewModel.NoCamerasFound;
        StatusText.Text = ViewModel.StatusMessage ?? string.Empty;

        // Update error
        if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
        {
            ErrorInfoBar.Message = ViewModel.ErrorMessage;
            ErrorInfoBar.IsOpen = true;
        }
        else
        {
            ErrorInfoBar.IsOpen = false;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GoBackCommand.Execute(null);
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadCamerasAsync();
    }

    private void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CameraComboBox.SelectedItem is CameraInfo camera)
        {
            ViewModel.SelectedCamera = camera;
            StatusText.Text = ViewModel.StatusMessage ?? string.Empty;
        }
    }
}
