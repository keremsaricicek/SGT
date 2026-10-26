using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sgt.Windows.App.Converters;

/// <summary>Değer boş (null veya boş metin) ise öğeyi gizler. Yalnızca görünüm içindir.</summary>
internal sealed class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null || value is string { Length: 0 } ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
