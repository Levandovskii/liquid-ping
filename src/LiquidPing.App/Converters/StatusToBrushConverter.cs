using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LiquidPing.Core.Models;

namespace LiquidPing.App.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    private static readonly Brush PENDING = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
    private static readonly Brush AVAILABLE = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
    private static readonly Brush UNSTABLE = new SolidColorBrush(Color.FromRgb(0xEF, 0x6C, 0x00));
    private static readonly Brush UNAVAILABLE = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is HostStatus status
            ? status switch
            {
                HostStatus.Available => AVAILABLE,
                HostStatus.Unstable => UNSTABLE,
                HostStatus.Unavailable => UNAVAILABLE,
                _ => PENDING
            }
            : PENDING;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
