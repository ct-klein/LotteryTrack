namespace LotteryTracker.App.Converters;

using LotteryTracker.Core.Entities;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Pending => new SolidColorBrush(Colors.Orange),
                TicketStatus.Winner => new SolidColorBrush(Colors.Green),
                TicketStatus.Loser => new SolidColorBrush(Colors.Gray),
                TicketStatus.Claimed => new SolidColorBrush(Colors.DodgerBlue),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
