namespace LotteryTracker.App.Services;

using System.Text.Json;
using Windows.Devices.Enumeration;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings _settings;

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LotteryTracker");
        Directory.CreateDirectory(appDataPath);
        _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        _settings = LoadSettings();
    }

    public string? SelectedCameraId
    {
        get => _settings.SelectedCameraId;
        set
        {
            _settings.SelectedCameraId = value;
            SaveSettings();
        }
    }

    public bool IsLoggingEnabled
    {
        get => _settings.IsLoggingEnabled;
        set
        {
            _settings.IsLoggingEnabled = value;
            SaveSettings();
        }
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

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // Return default settings if file can't be read
        }
        return new AppSettings();
    }

    private void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    private class AppSettings
    {
        public string? SelectedCameraId { get; set; }
        public bool IsLoggingEnabled { get; set; } = true;
    }
}
