namespace LotteryTracker.App.Converters;

using LotteryTracker.Core.Entities;
using Microsoft.UI.Xaml.Data;

public class StatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Pending => "Pending",
                TicketStatus.Winner => "Winner!",
                TicketStatus.Loser => "No Win",
                TicketStatus.Claimed => "Claimed",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
