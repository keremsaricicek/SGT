using System.Windows.Input;

namespace Sgt.Windows.App.Windows;

internal sealed partial class ShellWindow
{
    public ShellWindow() => InitializeComponent();

    /// <summary>Ctrl+F: arama kutusuna odaklan (yalnızca görünüm davranışı).</summary>
    private void OnFind(object sender, ExecutedRoutedEventArgs e)
    {
        FilterBox.Focus();
        FilterBox.SelectAll();
    }
}
