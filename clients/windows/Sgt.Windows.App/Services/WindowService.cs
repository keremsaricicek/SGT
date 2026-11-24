using System.Windows;
using Sgt.Windows.App.Windows;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.App.Services;

/// <summary>Her ekranı kendi penceresinde açar; aynı ekran ikinci kez açılmaz, mevcut pencere öne gelir.</summary>
internal sealed class WindowService : IWindowService
{
    private readonly Dictionary<string, ScreenWindow> _open = new(StringComparer.Ordinal);

    public bool TryActivate(string windowKey)
    {
        if (!_open.TryGetValue(windowKey, out var window))
        {
            return false;
        }

        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Activate();
        return true;
    }

    public void Show(string windowKey, IScreenViewModel viewModel)
    {
        var window = new ScreenWindow { DataContext = viewModel };
        window.Closed += (_, _) => _open.Remove(windowKey);
        _open[windowKey] = window;
        window.Show();
    }

    public void CloseAll()
    {
        foreach (var window in _open.Values.ToList())
        {
            window.Close();
        }
    }
}
