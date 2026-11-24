using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Sgt.Windows.App.Windows;
using Sgt.Windows.Core.Features.Login;
using Sgt.Windows.Core.Features.Shell;

namespace Sgt.Windows.App.Services;

/// <summary>Giriş penceresi ile ana pencere arasında geçiş. Ana pencere kapanınca uygulama kapanır.</summary>
internal sealed class AppNavigator(IServiceProvider services) : IAppNavigator
{
    public void ShowLogin() => Swap(new LoginWindow { DataContext = services.GetRequiredService<LoginViewModel>() });

    public void ShowShell()
    {
        var shell = services.GetRequiredService<ShellViewModel>();
        Swap(new ShellWindow { DataContext = shell });
        shell.LoadMenuCommand.Execute(null);
    }

    private static void Swap(Window next)
    {
        var previous = Application.Current.MainWindow;
        Application.Current.MainWindow = next;
        next.Show();
        if (previous is not null && !ReferenceEquals(previous, next))
        {
            previous.Close();
        }
    }
}
