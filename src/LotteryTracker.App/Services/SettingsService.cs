namespace LotteryTracker.App.Services;

using Windows.Devices.Enumeration;
using Windows.Storage;

public class SettingsService : ISettingsService
{
    private const string SelectedCameraIdKey = "SelectedCameraId";

    public string? SelectedCameraId
    {
        get => ApplicationData.Current.LocalSettings.Values[SelectedCameraIdKey] as string;
        set => ApplicationData.Current.LocalSettings.Values[SelectedCameraIdKey] = value;
    }

    public async Task<IReadOnlyList<CameraInfo>> GetAvailableCamerasAsync()
    {
        var cameras = new List<CameraInfo>();

        try
        {
            // Find all video capture devices
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            foreach (var device in devices)
            {
                cameras.Add(new CameraInfo(device.Id, device.Name));
            }
        }
        catch
        {
            // Return empty list if we can't enumerate devices
        }

        return cameras;
    }
}
