namespace LotteryTracker.App.Services;

public interface IBarcodeService
{
    Task<string?> ScanBarcodeAsync();
}
