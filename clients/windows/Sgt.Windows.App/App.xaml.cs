using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sgt.Windows.App.Services;
using Sgt.Windows.Core;
using Sgt.Windows.Core.Configuration;
using Sgt.Windows.Core.Features.Shell;

namespace Sgt.Windows.App;

/// <summary>
/// Uygulama girişi: ayarları okur, servisleri (DI) kurar, dili uygular ve giriş penceresini açar.
/// İş mantığı yoktur; tüm davranış Sgt.Windows.Core'daki ViewModel ve servislerdedir.
/// </summary>
internal sealed partial class App : Application, IDisposable
{
    private IHost? _host;

    public void Dispose() => _host?.Dispose();

    protected override void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = e.Args,
            ContentRootPath = AppContext.BaseDirectory,
        });
        builder.Services.AddSgtWindowsCore(builder.Configuration);
        builder.Services.AddSingleton<IWindowService, WindowService>();
        builder.Services.AddSingleton<IAppNavigator, AppNavigator>();
        _host = builder.Build();

        ApplyLanguage(_host.Services.GetRequiredService<IOptions<UiOptions>>().Value);
        _host.Services.GetRequiredService<IAppNavigator>().ShowLogin();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }

    /// <summary>Arayüz dili ve sayı/tarih biçimi. Varsayılan Türkçe; "en" İngilizce.</summary>
    private static void ApplyLanguage(UiOptions options)
    {
        var culture = string.Equals(options.Language, "en", StringComparison.OrdinalIgnoreCase)
            ? CultureInfo.GetCultureInfo("en-GB")
            : CultureInfo.GetCultureInfo("tr-TR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
    }
}
