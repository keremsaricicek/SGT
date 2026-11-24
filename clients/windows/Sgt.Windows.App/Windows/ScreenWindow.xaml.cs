using System.Windows.Input;

namespace Sgt.Windows.App.Windows;

internal sealed partial class ScreenWindow
{
    public ScreenWindow() => InitializeComponent();

    private void OnClose(object sender, ExecutedRoutedEventArgs e) => Close();
}
