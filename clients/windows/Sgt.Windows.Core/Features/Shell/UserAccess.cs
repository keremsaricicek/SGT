using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Features.Shell;

/// <summary>GET /api/v1/identity/me/menu satırı: görülebilen ekran ve o ekranda izin verilen eylemler.</summary>
[ApiSchema("MenuItem")]
public sealed record MenuItemDto(string Screen, IReadOnlyList<string> Actions);

/// <summary>Ekran kodları (sunucudaki ekran kataloğu ile aynı; docs/s-plus-ekranlar.md).</summary>
public static class ScreenCodes
{
    public const string PlayerSearch = "player.search";
    public const string PlayerProfile = "player.profile";
    public const string LiveTables = "live.tables";
    public const string LiveTableSession = "live.table_session";
    public const string LiveRating = "live.rating";
    public const string CageShift = "cage.shift";
    public const string CageChips = "cage.chips";
    public const string CageExchange = "cage.exchange";
    public const string CageAdjustment = "cage.adjustment";
    public const string CageSummary = "cage.summary";
}

/// <summary>
/// Kullanıcının yetkileri (menü yanıtından). Ekran "görebilir" ve "işlem" ayrı tutulur.
/// İstemci tarafı kontrol yalnızca kolaylıktır (buton gizleme/kapama); asıl kontrol API'dedir.
/// </summary>
public sealed class UserAccess
{
    private readonly Dictionary<string, HashSet<string>> _screens;

    private UserAccess(Dictionary<string, HashSet<string>> screens) => _screens = screens;

    public static UserAccess None { get; } = new([]);

    public IEnumerable<string> VisibleScreens => _screens.Keys;

    public static UserAccess From(IEnumerable<MenuItemDto> menu)
    {
        ArgumentNullException.ThrowIfNull(menu);
        var screens = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var item in menu)
        {
            if (!screens.TryGetValue(item.Screen, out var actions))
            {
                actions = new HashSet<string>(StringComparer.Ordinal);
                screens[item.Screen] = actions;
            }

            actions.UnionWith(item.Actions ?? []);
        }

        return new UserAccess(screens);
    }

    public bool CanView(string screen) => _screens.ContainsKey(screen);

    public bool CanDo(string screen, string action) =>
        _screens.TryGetValue(screen, out var actions) && actions.Contains(action);
}

/// <summary>Oturumdaki güncel yetkiler. Kabuk menüyü yükleyince ayarlar; ekranlar okur.</summary>
public interface ISessionAccess
{
    UserAccess Current { get; }

    void Set(UserAccess access);
}

internal sealed class SessionAccess : ISessionAccess
{
    public UserAccess Current { get; private set; } = UserAccess.None;

    public void Set(UserAccess access) => Current = access ?? throw new ArgumentNullException(nameof(access));
}

/// <summary>Kimlik modülü uçları (yalnızca istemcinin kullandığı kısım).</summary>
public interface IIdentityApi
{
    Task<ApiResult<IReadOnlyList<MenuItemDto>>> GetMyMenuAsync(CancellationToken ct);
}

internal sealed class IdentityApiClient(IApiTransport transport) : IIdentityApi
{
    public Task<ApiResult<IReadOnlyList<MenuItemDto>>> GetMyMenuAsync(CancellationToken ct) =>
        transport.GetAsync<IReadOnlyList<MenuItemDto>>(ApiRoutes.MyMenu.Build(), ct);
}
