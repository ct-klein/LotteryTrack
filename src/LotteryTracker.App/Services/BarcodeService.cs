namespace LotteryTracker.App.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using ZXing;
using ZXing.Common;
using System.Runtime.InteropServices.WindowsRuntime;

public class BarcodeService : IBarcodeService
{
    private readonly Window _mainWindow;

    public BarcodeService(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task<string?> ScanBarcodeAsync()
    {
        var dialog = new BarcodeScannerDialog();
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
    private MediaCapture? _mediaCapture;
    private MediaFrameReader? _frameReader;
    private BarcodeReaderGeneric? _barcodeReader;
    private bool _isScanning;

    public string? ScannedBarcode { get; private set; }

    public BarcodeScannerDialog()
    {
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

            // Find camera
            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
            var sourceGroup = frameSourceGroups.FirstOrDefault(g =>
                g.SourceInfos.Any(s => s.SourceKind == MediaFrameSourceKind.Color));

            if (sourceGroup == null)
            {
                ShowCameraError("No camera found on this device.");
                return;
            }

            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                SourceGroup = sourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };

            await _mediaCapture.InitializeAsync(settings);

            var colorSourceInfo = sourceGroup.SourceInfos
                .FirstOrDefault(s => s.SourceKind == MediaFrameSourceKind.Color);

            if (colorSourceInfo == null)
            {
                ShowCameraError("No color camera source found.");
                return;
            }

            var colorSource = _mediaCapture.FrameSources[colorSourceInfo.Id];
            _frameReader = await _mediaCapture.CreateFrameReaderAsync(colorSource, MediaEncodingSubtypes.Bgra8);
            _frameReader.FrameArrived += FrameReader_FrameArrived;

            await _frameReader.StartAsync();
            _isScanning = true;

            // Update UI to show scanning state
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
                    new ProgressRing { IsActive = true, Width = 32, Height = 32 }
                }
            };
        }
        catch (UnauthorizedAccessException)
        {
            ShowCameraError("Camera access denied. Please enable camera permissions in Settings.");
        }
        catch (Exception ex)
        {
            ShowCameraError($"Camera error: {ex.Message}");
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

    private void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        if (!_isScanning || _barcodeReader == null)
            return;

        using var frameReference = sender.TryAcquireLatestFrame();
        if (frameReference?.VideoMediaFrame?.SoftwareBitmap == null)
            return;

        try
        {
            using var bitmap = SoftwareBitmap.Convert(
                frameReference.VideoMediaFrame.SoftwareBitmap,
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var buffer = new byte[width * height * 4];
            bitmap.CopyToBuffer(buffer.AsBuffer());

            // Create luminance source from BGRA data
            var luminanceSource = new RGBLuminanceSource(buffer, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);
            var result = _barcodeReader.Decode(luminanceSource);

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                _isScanning = false;
                ScannedBarcode = result.Text;

                // Close dialog on UI thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    Hide();
                });
            }
        }
        catch
        {
            // Ignore decoding errors, continue scanning
        }
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
