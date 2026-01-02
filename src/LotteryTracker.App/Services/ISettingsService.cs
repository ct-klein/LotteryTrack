namespace LotteryTracker.App.Services;

public interface ISettingsService
{
    string? SelectedCameraId { get; set; }
    bool IsLoggingEnabled { get; set; }
    Task<IReadOnlyList<CameraInfo>> GetAvailableCamerasAsync();
}

public record CameraInfo(string Id, string Name);
