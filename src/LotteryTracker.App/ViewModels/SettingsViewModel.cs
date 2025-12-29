namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;
using System.Collections.ObjectModel;

public partial class SettingsViewModel(
    ISettingsService settingsService,
    INavigationService navigationService) : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<CameraInfo> _availableCameras = [];

    [ObservableProperty]
    private CameraInfo? _selectedCamera;

    [ObservableProperty]
    private bool _noCamerasFound;

    [ObservableProperty]
    private string? _statusMessage;

    public async Task LoadCamerasAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;
            NoCamerasFound = false;

            var cameras = await settingsService.GetAvailableCamerasAsync();

            AvailableCameras.Clear();

            if (cameras.Count == 0)
            {
                NoCamerasFound = true;
                StatusMessage = "No cameras detected on this device.";
                return;
            }

            foreach (var camera in cameras)
            {
                AvailableCameras.Add(camera);
            }

            // Select the previously saved camera, or the first one
            var savedCameraId = settingsService.SelectedCameraId;
            SelectedCamera = cameras.FirstOrDefault(c => c.Id == savedCameraId)
                ?? cameras.FirstOrDefault();

            StatusMessage = $"Found {cameras.Count} camera(s)";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load cameras: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedCameraChanged(CameraInfo? value)
    {
        if (value != null)
        {
            settingsService.SelectedCameraId = value.Id;
            StatusMessage = $"Camera saved: {value.Name}";
        }
    }

    [RelayCommand]
    private async Task RefreshCamerasAsync()
    {
        await LoadCamerasAsync();
    }

    [RelayCommand]
    private void GoBack() => navigationService.GoBack();
}
