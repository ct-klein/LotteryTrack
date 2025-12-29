namespace LotteryTracker.App.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Windows.Devices.Enumeration;
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
    private MediaFrameReader? _frameReader;
    private BarcodeReaderGeneric? _barcodeReader;
    private bool _isScanning;

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

            // Find available frame source groups for video capture
            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
            MediaFrameSourceGroup? selectedGroup = null;
            MediaFrameSourceInfo? colorSourceInfo = null;

            // If a specific camera is selected, try to find its frame source group
            if (!string.IsNullOrEmpty(_selectedCameraId))
            {
                foreach (var group in frameSourceGroups)
                {
                    var matchingSource = group.SourceInfos.FirstOrDefault(
                        s => s.SourceKind == MediaFrameSourceKind.Color &&
                             s.DeviceInformation?.Id == _selectedCameraId);

                    if (matchingSource != null)
                    {
                        selectedGroup = group;
                        colorSourceInfo = matchingSource;
                        break;
                    }
                }
            }

            // Fall back to first available color source
            if (selectedGroup == null)
            {
                foreach (var group in frameSourceGroups)
                {
                    var firstColorSource = group.SourceInfos.FirstOrDefault(
                        s => s.SourceKind == MediaFrameSourceKind.Color);

                    if (firstColorSource != null)
                    {
                        selectedGroup = group;
                        colorSourceInfo = firstColorSource;
                        break;
                    }
                }
            }

            if (selectedGroup == null || colorSourceInfo == null)
            {
                // Try alternate method - direct device enumeration
                await InitializeCameraAlternateAsync();
                return;
            }

            // Initialize MediaCapture with the frame source group
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                SourceGroup = selectedGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };

            await _mediaCapture.InitializeAsync(settings);

            // Get the color frame source and create a reader
            var colorSource = _mediaCapture.FrameSources[colorSourceInfo.Id];

            // Try to find a format that works
            var preferredFormat = colorSource.SupportedFormats
                .Where(f => f.VideoFormat.Width >= 640)
                .OrderByDescending(f => f.VideoFormat.Width)
                .FirstOrDefault();

            if (preferredFormat != null)
            {
                await colorSource.SetFormatAsync(preferredFormat);
            }

            _frameReader = await _mediaCapture.CreateFrameReaderAsync(colorSource);
            _frameReader.FrameArrived += FrameReader_FrameArrived;

            var status = await _frameReader.StartAsync();
            if (status != MediaFrameReaderStartStatus.Success)
            {
                // Fall back to alternate method
                await CleanupCurrentCaptureAsync();
                await InitializeCameraAlternateAsync();
                return;
            }

            _isScanning = true;

            // Get camera name for display
            var cameraName = colorSourceInfo.DeviceInformation?.Name ?? "camera";

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
        }
        catch
        {
            // Try alternate method on any error
            try
            {
                await CleanupCurrentCaptureAsync();
                await InitializeCameraAlternateAsync();
            }
            catch (Exception altEx)
            {
                ShowCameraError($"Camera error: {altEx.Message}\n\nTip: Go to Settings to select a different camera.");
            }
        }
    }

    private async Task InitializeCameraAlternateAsync()
    {
        try
        {
            // Find video capture devices directly
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            if (devices.Count == 0)
            {
                ShowCameraError("No cameras found on this device.\n\nMake sure your camera is connected and has drivers installed.");
                return;
            }

            // Select the device
            DeviceInformation? selectedDevice = null;
            if (!string.IsNullOrEmpty(_selectedCameraId))
            {
                selectedDevice = devices.FirstOrDefault(d => d.Id == _selectedCameraId);
            }
            selectedDevice ??= devices.FirstOrDefault();

            if (selectedDevice == null)
            {
                ShowCameraError("Could not access the selected camera.");
                return;
            }

            // Initialize MediaCapture with the selected device
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = selectedDevice.Id,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MediaCategory = MediaCategory.Other
            };

            await _mediaCapture.InitializeAsync(settings);

            // Check if video preview is available
            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            if (previewProperties == null)
            {
                ShowCameraError($"Camera '{selectedDevice.Name}' does not support video preview.\n\nTry selecting a different camera in Settings.");
                return;
            }

            _isScanning = true;

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
                        Text = $"Using: {selectedDevice.Name}",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 12
                    },
                    new ProgressRing { IsActive = true, Width = 32, Height = 32 }
                }
            };

            // Use timer-based capture with video preview
            _ = CaptureLoopAsync();
        }
        catch (UnauthorizedAccessException)
        {
            ShowCameraError("Camera access denied. Please enable camera permissions in Windows Settings.");
        }
        catch (Exception ex)
        {
            ShowCameraError($"Camera error: {ex.Message}\n\nTip: Go to Settings to select a different camera.");
        }
    }

    private async Task CaptureLoopAsync()
    {
        try
        {
            while (_isScanning && _mediaCapture != null)
            {
                try
                {
                    // Capture a frame from video preview
                    var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                    if (previewProperties == null)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    var videoFrame = new Windows.Media.VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
                    await _mediaCapture.GetPreviewFrameAsync(videoFrame);

                    if (videoFrame.SoftwareBitmap != null)
                    {
                        var softwareBitmap = videoFrame.SoftwareBitmap;

                        // Convert if needed
                        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8)
                        {
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        }

                        var width = softwareBitmap.PixelWidth;
                        var height = softwareBitmap.PixelHeight;
                        var buffer = new byte[width * height * 4];
                        softwareBitmap.CopyToBuffer(buffer.AsBuffer());

                        // Try to decode barcode
                        var luminanceSource = new RGBLuminanceSource(buffer, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);
                        var result = _barcodeReader?.Decode(luminanceSource);

                        if (result != null && !string.IsNullOrEmpty(result.Text))
                        {
                            _isScanning = false;
                            ScannedBarcode = result.Text;

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                Hide();
                            });
                            return;
                        }

                        softwareBitmap.Dispose();
                    }

                    videoFrame.Dispose();
                }
                catch
                {
                    // Ignore individual capture errors
                }

                await Task.Delay(200);
            }
        }
        catch
        {
            // Loop ended
        }
    }

    private void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        if (!_isScanning || _barcodeReader == null)
            return;

        try
        {
            using var frameReference = sender.TryAcquireLatestFrame();
            if (frameReference?.VideoMediaFrame?.SoftwareBitmap == null)
                return;

            using var softwareBitmap = SoftwareBitmap.Convert(
                frameReference.VideoMediaFrame.SoftwareBitmap,
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            var width = softwareBitmap.PixelWidth;
            var height = softwareBitmap.PixelHeight;
            var buffer = new byte[width * height * 4];
            softwareBitmap.CopyToBuffer(buffer.AsBuffer());

            var luminanceSource = new RGBLuminanceSource(buffer, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);
            var result = _barcodeReader.Decode(luminanceSource);

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                _isScanning = false;
                ScannedBarcode = result.Text;

                DispatcherQueue.TryEnqueue(() =>
                {
                    Hide();
                });
            }
        }
        catch
        {
            // Ignore decoding errors
        }
    }

    private async Task CleanupCurrentCaptureAsync()
    {
        if (_frameReader != null)
        {
            _frameReader.FrameArrived -= FrameReader_FrameArrived;
            await _frameReader.StopAsync();
            _frameReader.Dispose();
            _frameReader = null;
        }

        if (_mediaCapture != null)
        {
            _mediaCapture.Dispose();
            _mediaCapture = null;
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

        if (_frameReader != null)
        {
            _frameReader.FrameArrived -= FrameReader_FrameArrived;
            _frameReader.Dispose();
            _frameReader = null;
        }

        if (_mediaCapture != null)
        {
            _mediaCapture.Dispose();
            _mediaCapture = null;
        }
    }
}
