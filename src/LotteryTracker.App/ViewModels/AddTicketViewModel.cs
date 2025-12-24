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
    private DateTime _purchaseDate = DateTime.Today;

    [ObservableProperty]
    private decimal _price;

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
    private int? _ticketNumber;

    // Draw game specific
    [ObservableProperty]
    private DrawGameType _selectedGameType = DrawGameType.Powerball;

    [ObservableProperty]
    private string? _customGameName;

    [ObservableProperty]
    private string? _numbersSelected;

    [ObservableProperty]
    private string? _bonusNumbers;

    [ObservableProperty]
    private DateTime _drawDate = DateTime.Today;

    [ObservableProperty]
    private bool _isQuickPick;

    [ObservableProperty]
    private int _numberOfDraws = 1;

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        // TODO: Implement barcode scanning
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveTicketAsync()
    {
        try
        {
            IsLoading = true;

            Ticket ticket = SelectedTicketType switch
            {
                TicketType.ScratchOff => new ScratchOffTicket
                {
                    SerialNumber = SerialNumber,
                    PurchaseDate = PurchaseDate,
                    Price = Price,
                    StoreName = StoreName,
                    StoreLocation = StoreLocation,
                    Notes = Notes,
                    GameName = GameName ?? string.Empty,
                    GameNumber = GameNumber,
                    TicketNumber = TicketNumber
                },
                TicketType.DrawGame => new DrawGameTicket
                {
                    SerialNumber = SerialNumber,
                    PurchaseDate = PurchaseDate,
                    Price = Price,
                    StoreName = StoreName,
                    StoreLocation = StoreLocation,
                    Notes = Notes,
                    GameType = SelectedGameType,
                    CustomGameName = CustomGameName,
                    NumbersSelected = NumbersSelected ?? string.Empty,
                    BonusNumbers = BonusNumbers,
                    DrawDate = DrawDate,
                    IsQuickPick = IsQuickPick,
                    NumberOfDraws = NumberOfDraws
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
