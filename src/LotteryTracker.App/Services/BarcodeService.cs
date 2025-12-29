namespace LotteryTracker.App.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using ZXing;
using ZXing.Common;
using System.Runtime.InteropServices.WindowsRuntime;

public class BarcodeService : IBarcodeService
{
    private readonly Window _mainWindow;
    private readonly ISettingsService _settingsService;

    public BarcodeService(Window mainWindow, ISettingsService settingsService)
    {
        _mainWindow = mainWindow;
        _settingsService = settingsService;
    }

    public async Task<string?> ScanBarcodeAsync()
    {
        var selectedCameraId = _settingsService.SelectedCameraId;
        var dialog = new BarcodeScannerDialog(selectedCameraId);
        dialog.XamlRoot = _mainWindow.Content.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return dialog.ScannedBarcode;
        }

        return null;
    }
}

public sealed partial class BarcodeScannerDialog : ContentDialog
{
    private readonly string? _selectedCameraId;
    private MediaCapture? _mediaCapture;
    private BarcodeReaderGeneric? _barcodeReader;
    private bool _isScanning;
    private CancellationTokenSource? _scanCancellation;

    public string? ScannedBarcode { get; private set; }

    public BarcodeScannerDialog(string? selectedCameraId = null)
    {
        _selectedCameraId = selectedCameraId;

        Title = "Scan Barcode";
        PrimaryButtonText = "Cancel";
        DefaultButton = ContentDialogButton.Primary;

        var stackPanel = new StackPanel
        {
            Spacing = 16,
            MinWidth = 400,
            MinHeight = 350
        };

        // Camera preview placeholder
        var previewBorder = new Border
        {
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black),
            CornerRadius = new CornerRadius(8),
            MinHeight = 280,
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 12,
                Children =
                {
                    new ProgressRing { IsActive = true, Width = 48, Height = 48 },
                    new TextBlock
                    {
                        Text = "Initializing camera...",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
            }
        };

        _previewBorder = previewBorder;

        var instructionText = new TextBlock
        {
            Text = "Position the barcode within the camera view",
            TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
        };

        // Manual entry option
        var manualEntryPanel = new StackPanel { Spacing = 8 };
        manualEntryPanel.Children.Add(new TextBlock
        {
            Text = "Or enter manually:",
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
        });

        var manualEntryBox = new TextBox
        {
            PlaceholderText = "Enter barcode number"
        };
        _manualEntryBox = manualEntryBox;

        var useManualButton = new Button
        {
            Content = "Use This Code",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        useManualButton.Click += UseManualButton_Click;

        manualEntryPanel.Children.Add(manualEntryBox);
        manualEntryPanel.Children.Add(useManualButton);

        stackPanel.Children.Add(previewBorder);
        stackPanel.Children.Add(instructionText);
        stackPanel.Children.Add(manualEntryPanel);

        Content = stackPanel;

        Loaded += BarcodeScannerDialog_Loaded;
        Closing += BarcodeScannerDialog_Closing;
    }

    private Border _previewBorder = null!;
    private TextBox _manualEntryBox = null!;

    private void UseManualButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_manualEntryBox.Text))
        {
            ScannedBarcode = _manualEntryBox.Text.Trim();
            Hide();
        }
    }

    private async void BarcodeScannerDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeCameraAsync();
    }

    private void BarcodeScannerDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        StopScanning();
    }

    private async Task InitializeCameraAsync()
    {
        try
        {
            _barcodeReader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.DATA_MATRIX,
                        BarcodeFormat.PDF_417
                    }
                }
            };

            // Initialize MediaCapture with settings
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MediaCategory = MediaCategory.Other,
                PhotoCaptureSource = PhotoCaptureSource.Auto
            };

            // Use selected camera if specified
            if (!string.IsNullOrEmpty(_selectedCameraId))
            {
                settings.VideoDeviceId = _selectedCameraId;
            }

            await _mediaCapture.InitializeAsync(settings);

            _isScanning = true;
            _scanCancellation = new CancellationTokenSource();

            // Update UI to show scanning state with camera name if available
            var cameraName = "camera";
            if (_mediaCapture.MediaCaptureSettings.VideoDeviceId != null)
            {
                try
                {
                    var device = await Windows.Devices.Enumeration.DeviceInformation.CreateFromIdAsync(
                        _mediaCapture.MediaCaptureSettings.VideoDeviceId);
                    cameraName = device.Name;
                }
                catch
                {
                    // Ignore errors getting camera name
                }
            }

            _previewBorder.Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 12,
                Children =
                {
                    new FontIcon
                    {
                        Glyph = "\uED1A",
                        FontSize = 64,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                    },
                    new TextBlock
                    {
                        Text = "Scanning for barcodes...",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = $"Using: {cameraName}",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 12
                    },
                    new ProgressRing { IsActive = true, Width = 32, Height = 32 }
                }
            };

            // Start scanning loop
            _ = ScanLoopAsync(_scanCancellation.Token);
        }
        catch (UnauthorizedAccessException)
        {
            ShowCameraError("Camera access denied. Please enable camera permissions in Settings.");
        }
        catch (Exception ex)
        {
            ShowCameraError($"Camera error: {ex.Message}\n\nTip: Go to Settings to select a different camera.");
        }
    }

    private async Task ScanLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Prepare low-lag photo capture with uncompressed format for direct SoftwareBitmap access
            var imageProperties = ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8);
            var lowLagCapture = await _mediaCapture!.PrepareLowLagPhotoCaptureAsync(imageProperties);

            while (_isScanning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var capturedPhoto = await lowLagCapture.CaptureAsync();
                    var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

                    if (softwareBitmap != null)
                    {
                        // Convert to BGRA8 if needed
                        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8)
                        {
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        }

                        var width = softwareBitmap.PixelWidth;
                        var height = softwareBitmap.PixelHeight;
                        var buffer = new byte[width * height * 4];
                        softwareBitmap.CopyToBuffer(buffer.AsBuffer());
                        softwareBitmap.Dispose();

                        // Try to decode barcode
                        var luminanceSource = new RGBLuminanceSource(buffer, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);
                        var result = _barcodeReader!.Decode(luminanceSource);

                        if (result != null && !string.IsNullOrEmpty(result.Text))
                        {
                            _isScanning = false;
                            ScannedBarcode = result.Text;

                            await lowLagCapture.FinishAsync();

                            // Close dialog on UI thread
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                Hide();
                            });
                            return;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Ignore individual frame errors, continue scanning
                }

                // Small delay between captures
                await Task.Delay(200, cancellationToken);
            }

            await lowLagCapture.FinishAsync();
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, ignore
        }
        catch (Exception ex)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ShowCameraError($"Scanning error: {ex.Message}");
            });
        }
    }

    private void ShowCameraError(string message)
    {
        _previewBorder.Child = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 12,
            Children =
            {
                new FontIcon
                {
                    Glyph = "\uE783",
                    FontSize = 48,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange)
                },
                new TextBlock
                {
                    Text = message,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
                    MaxWidth = 350
                },
                new TextBlock
                {
                    Text = "Use manual entry below instead.",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            }
        };
    }

    private void StopScanning()
    {
        _isScanning = false;
        _scanCancellation?.Cancel();
        _scanCancellation?.Dispose();
        _scanCancellation = null;

        if (_mediaCapture != null)
        {
            _mediaCapture.Dispose();
            _mediaCapture = null;
        }
    }
}
