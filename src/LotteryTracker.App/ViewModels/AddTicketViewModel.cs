namespace LotteryTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LotteryTracker.App.Services;
using LotteryTracker.Core.Entities;
using LotteryTracker.Core.Interfaces;

public partial class AddTicketViewModel(
    ITicketRepository ticketRepository,
    INavigationService navigationService) : BaseViewModel
{
    [ObservableProperty]
    private TicketType _selectedTicketType = TicketType.ScratchOff;

    [ObservableProperty]
    private string? _serialNumber;

    [ObservableProperty]
    private DateTimeOffset _purchaseDate = DateTimeOffset.Now;

    [ObservableProperty]
    private double _price;

    [ObservableProperty]
    private string? _storeName;

    [ObservableProperty]
    private string? _storeLocation;

    [ObservableProperty]
    private string? _notes;

    // Scratch-off specific
    [ObservableProperty]
    private string? _gameName;

    [ObservableProperty]
    private string? _gameNumber;

    [ObservableProperty]
    private double _ticketNumberValue = double.NaN;

    // Draw game specific
    [ObservableProperty]
    private int _selectedGameTypeIndex;

    [ObservableProperty]
    private string? _customGameName;

    [ObservableProperty]
    private string? _numbersSelected;

    [ObservableProperty]
    private string? _bonusNumbers;

    [ObservableProperty]
    private DateTimeOffset _drawDate = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _isQuickPick;

    [ObservableProperty]
    private double _numberOfDraws = 1;

    private DrawGameType GetGameTypeFromIndex(int index) => index switch
    {
        0 => DrawGameType.Powerball,
        1 => DrawGameType.MegaMillions,
        2 => DrawGameType.StateLottery,
        3 => DrawGameType.Pick3,
        4 => DrawGameType.Pick4,
        5 => DrawGameType.Cash5,
        6 => DrawGameType.Other,
        _ => DrawGameType.Powerball
    };

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        // TODO: Implement barcode scanning in Phase 8
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveTicketAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Validation
            if (Price <= 0)
            {
                ErrorMessage = "Please enter a valid price.";
                return;
            }

            if (SelectedTicketType == TicketType.ScratchOff && string.IsNullOrWhiteSpace(GameName))
            {
                ErrorMessage = "Please enter a game name for the scratch-off ticket.";
                return;
            }

            Ticket ticket = SelectedTicketType switch
            {
                TicketType.ScratchOff => new ScratchOffTicket
                {
                    SerialNumber = SerialNumber,
                    PurchaseDate = PurchaseDate.DateTime,
                    Price = (decimal)Price,
                    StoreName = StoreName,
                    StoreLocation = StoreLocation,
                    Notes = Notes,
                    GameName = GameName ?? string.Empty,
                    GameNumber = GameNumber,
                    TicketNumber = double.IsNaN(TicketNumberValue) ? null : (int)TicketNumberValue
                },
                TicketType.DrawGame => new DrawGameTicket
                {
                    SerialNumber = SerialNumber,
                    PurchaseDate = PurchaseDate.DateTime,
                    Price = (decimal)Price,
                    StoreName = StoreName,
                    StoreLocation = StoreLocation,
                    Notes = Notes,
                    GameType = GetGameTypeFromIndex(SelectedGameTypeIndex),
                    CustomGameName = CustomGameName,
                    NumbersSelected = NumbersSelected ?? string.Empty,
                    BonusNumbers = BonusNumbers,
                    DrawDate = DrawDate.DateTime,
                    IsQuickPick = IsQuickPick,
                    NumberOfDraws = (int)NumberOfDraws
                },
                _ => throw new InvalidOperationException("Invalid ticket type")
            };

            await ticketRepository.AddTicketAsync(ticket);
            navigationService.GoBack();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel() => navigationService.GoBack();
}
