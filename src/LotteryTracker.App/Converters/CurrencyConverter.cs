namespace LotteryTracker.App.Converters;

using Microsoft.UI.Xaml.Data;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal amount)
        {
            return amount.ToString("C2");
        }
        if (value is decimal?)
        {
            var nullableAmount = (decimal?)value;
            if (nullableAmount.HasValue)
            {
                return nullableAmount.Value.ToString("C2");
            }
        }
        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && decimal.TryParse(str, out var result))
        {
            return result;
        }
        return 0m;
    }
}
