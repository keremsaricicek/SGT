using Microsoft.Extensions.DependencyInjection;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Shell;

/// <summary>Bir ekranı açma isteği. <see cref="EntityId"/>: ekran belirli bir kayıt için açılıyorsa (ör. oyuncu kartı).</summary>
public sealed record ScreenRequest(string ScreenCode, Guid? EntityId = null)
{
    /// <summary>Pencere anahtarı: aynı ekran + kayıt ikinci kez açılmaz, mevcut pencere öne gelir.</summary>
    public string WindowKey => EntityId is { } id ? $"{ScreenCode}/{id:D}" : ScreenCode;
}

/// <summary>
/// Windows istemcisinin açabildiği bir ekran. Her özellik kendi ekranlarını kaydeder (yeni ekran = yeni kayıt, OCP).
/// <see cref="ShowInMenu"/> false ise ekran yalnızca başka ekrandan açılır (ör. oyuncu kartı aramadan).
/// </summary>
public sealed record ScreenDescriptor(
    string ScreenCode,
    Func<string> Title,
    Func<IServiceProvider, ScreenRequest, IScreenViewModel> Create,
    bool ShowInMenu = true);

/// <summary>Kayıtlı ekranlar.</summary>
public interface IScreenCatalog
{
    IReadOnlyList<ScreenDescriptor> All { get; }

    ScreenDescriptor? Find(string screenCode);
}

internal sealed class ScreenCatalog(IEnumerable<ScreenDescriptor> screens) : IScreenCatalog
{
    public IReadOnlyList<ScreenDescriptor> All { get; } = screens.ToList();

    public ScreenDescriptor? Find(string screenCode) => All.FirstOrDefault(s => s.ScreenCode == screenCode);
}

/// <summary>Ekranları ayrı pencerelerde gösterir ("pencereler"). WPF uygulaması gerçekler.</summary>
public interface IWindowService
{
    /// <summary>Bu anahtarla açık pencere varsa öne getirir ve true döner.</summary>
    bool TryActivate(string windowKey);

    void Show(string windowKey, IScreenViewModel viewModel);

    void CloseAll();
}

/// <summary>Uygulama düzeyi geçişler: giriş penceresi ↔ ana pencere.</summary>
public interface IAppNavigator
{
    void ShowShell();

    void ShowLogin();
}

/// <summary>Ekran açma: yetki kontrolü, tekil pencere, ViewModel oluşturma ve ilk yükleme.</summary>
public interface IScreenOpener
{
    Task<bool> OpenAsync(ScreenRequest request, CancellationToken ct);
}

internal sealed class ScreenOpener(IScreenCatalog catalog, ISessionAccess access, IWindowService windows, IServiceProvider services)
    : IScreenOpener
{
    public async Task<bool> OpenAsync(ScreenRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var descriptor = catalog.Find(request.ScreenCode);
        if (descriptor is null || !access.Current.CanView(request.ScreenCode))
        {
            return false;
        }

        if (windows.TryActivate(request.WindowKey))
        {
            return true;
        }

        var viewModel = descriptor.Create(services, request);
        windows.Show(request.WindowKey, viewModel);
        await viewModel.InitializeAsync(ct);
        return true;
    }
}

/// <summary>Özellik kayıtlarında kullanılan kısayol.</summary>
public static class ScreenRegistration
{
    public static IServiceCollection AddScreen<TViewModel>(
        this IServiceCollection services, string screenCode, Func<string> title, bool showInMenu = true)
        where TViewModel : class, IScreenViewModel
    {
        services.AddTransient<TViewModel>();
        return services.AddSingleton(new ScreenDescriptor(
            screenCode, title, (sp, _) => sp.GetRequiredService<TViewModel>(), showInMenu));
    }
}
