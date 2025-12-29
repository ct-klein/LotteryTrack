namespace LotteryTracker.App.Views;

using LotteryTracker.App.ViewModels;
using LotteryTracker.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class AddTicketPage : Page
{
    public AddTicketViewModel ViewModel { get; }

    public AddTicketPage()
    {
        ViewModel = App.Services.GetRequiredService<AddTicketViewModel>();
        this.InitializeComponent();

        // Set initial date to today
        PurchaseDatePicker.Date = DateTimeOffset.Now;
        GameTypeCombo.SelectedIndex = 0;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ErrorMessage))
        {
            ErrorInfoBar.Message = ViewModel.ErrorMessage ?? "";
            ErrorInfoBar.IsOpen = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
        }
    }

    private void TicketTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TicketTypeSelector.SelectedIndex == 0)
        {
            ViewModel.SelectedTicketType = TicketType.ScratchOff;
            ScratchOffFields.Visibility = Visibility.Visible;
            DrawGameFields.Visibility = Visibility.Collapsed;
        }
        else
        {
            ViewModel.SelectedTicketType = TicketType.DrawGame;
            ScratchOffFields.Visibility = Visibility.Collapsed;
            DrawGameFields.Visibility = Visibility.Visible;
        }
    }

    private void SerialNumberBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.SerialNumber = SerialNumberBox.Text;
    }

    private void ScanBarcodeButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ScanBarcodeCommand.Execute(null);
    }

    private void PurchaseDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        ViewModel.PurchaseDate = PurchaseDatePicker.Date ?? DateTimeOffset.Now;
    }

    private void PriceBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.Price = double.IsNaN(args.NewValue) ? 0 : args.NewValue;
    }

    private void StoreNameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.StoreName = StoreNameBox.Text;
    }

    private void StoreLocationBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.StoreLocation = StoreLocationBox.Text;
    }

    private void GameNameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.GameName = GameNameBox.Text;
    }

    private void GameNumberBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.GameNumber = GameNumberBox.Text;
    }

    private void TicketNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.TicketNumberValue = double.IsNaN(args.NewValue) ? 0 : args.NewValue;
    }

    private void GameTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedGameTypeIndex = GameTypeCombo.SelectedIndex;
        CustomGameNameField.Visibility = GameTypeCombo.SelectedIndex == 6 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CustomGameNameField_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.CustomGameName = CustomGameNameField.Text;
    }

    private void NumbersSelectedBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.NumbersSelected = NumbersSelectedBox.Text;
    }

    private void BonusNumbersBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.BonusNumbers = BonusNumbersBox.Text;
    }

    private void DrawDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        ViewModel.DrawDate = DrawDatePicker.Date ?? DateTimeOffset.Now;
    }

    private void QuickPickCheck_Changed(object sender, RoutedEventArgs e)
    {
        ViewModel.IsQuickPick = QuickPickCheck.IsChecked ?? false;
    }

    private void NumberOfDrawsBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.NumberOfDraws = double.IsNaN(args.NewValue) ? 1 : (int)args.NewValue;
    }

    private void NotesBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.Notes = NotesBox.Text;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SaveTicketCommand.Execute(null);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelCommand.Execute(null);
    }
}
