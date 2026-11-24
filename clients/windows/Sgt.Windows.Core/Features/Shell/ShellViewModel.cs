using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Auth;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Shell;

/// <summary>Menüdeki bir ekran.</summary>
public sealed record MenuEntry(string ScreenCode, string Title, string Group);

/// <summary>
/// Ana pencere (Y2): menü kullanıcının yetkisine göre sunucudan gelir (/api/v1/identity/me/menu).
/// Yalnızca hem yetkili hem de bu istemcide karşılığı olan ekranlar listelenir. Her ekran ayrı pencerede açılır.
/// </summary>
public sealed partial class ShellViewModel : StatusViewModel
{
    private readonly IIdentityApi _identity;
    private readonly ISessionAccess _access;
    private readonly IScreenCatalog _catalog;
    private readonly IScreenOpener _opener;
    private readonly IWindowService _windows;
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;
    private readonly ICashierShiftContext _shift;
    private readonly IReadOnlyList<ISessionInitializer> _initializers;
    private List<MenuEntry> _allEntries = [];

    public ShellViewModel(
        IIdentityApi identity,
        ISessionAccess access,
        IScreenCatalog catalog,
        IScreenOpener opener,
        IWindowService windows,
        IAuthService auth,
        IAppNavigator navigator,
        ICashierShiftContext shift,
        ITerminalInfo terminal,
        IEnumerable<ISessionInitializer> initializers)
    {
        ArgumentNullException.ThrowIfNull(initializers);
        _initializers = initializers.ToList();
        ArgumentNullException.ThrowIfNull(shift);
        ArgumentNullException.ThrowIfNull(terminal);
        _identity = identity;
        _access = access;
        _catalog = catalog;
        _opener = opener;
        _windows = windows;
        _auth = auth;
        _navigator = navigator;
        _shift = shift;
        Terminal = terminal.Name;
        _shift.Changed += (_, _) => OnPropertyChanged(nameof(CurrentShift));
    }

    public ObservableCollection<MenuEntry> MenuEntries { get; } = [];

    public string Terminal { get; }

    public string UserName => _auth.CurrentUser?.DisplayName ?? string.Empty;

    public string CurrentShift => _shift.ShiftId is { } id ? id.ToString("D")[..8] : Strings.Shell_NoShift;

    [ObservableProperty]
    public partial MenuEntry? SelectedEntry { get; set; }

    [ObservableProperty]
    public partial string? Filter { get; set; }

    partial void OnFilterChanged(string? value) => ApplyFilter();

    [RelayCommand]
    private async Task LoadMenuAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var result = await _identity.GetMyMenuAsync(ct);
            if (!result.IsSuccess)
            {
                _access.Set(UserAccess.None);
                _allEntries = [];
                ApplyFilter();
                ShowProblem(result.Problem!);
                return;
            }

            var access = UserAccess.From(result.Value);
            _access.Set(access);
            _allEntries = _catalog.All
                .Where(s => s.ShowInMenu && access.CanView(s.ScreenCode))
                .Select(s => new MenuEntry(s.ScreenCode, s.Title(), GroupOf(s.ScreenCode)))
                .ToList();
            ApplyFilter();
            OnPropertyChanged(nameof(UserName));
            if (_allEntries.Count == 0)
            {
                ShowStatus(Strings.Shell_NoScreens);
            }
            else
            {
                ClearMessages();
            }

            // Oturum hazırlığı (ör. açık vardiyaya otomatik bağlanma). Hata menüyü engellemez, yalnızca gösterilir.
            foreach (var initializer in _initializers)
            {
                if (await initializer.InitializeAsync(access, ct) is { } problem)
                {
                    ShowProblem(problem);
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(MenuEntry? entry)
    {
        var target = entry ?? SelectedEntry;
        if (target is null)
        {
            return;
        }

        if (!await _opener.OpenAsync(new ScreenRequest(target.ScreenCode), CancellationToken.None))
        {
            ShowError(Strings.Shell_NoAccess);
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _windows.CloseAll();
        _access.Set(UserAccess.None);
        _shift.Detach();
        await _auth.LogoutAsync(CancellationToken.None);
        _navigator.ShowLogin();
    }

    private static string GroupOf(string screenCode) => screenCode.Split('.')[0] switch
    {
        "cage" => Strings.Group_Cage,
        "player" => Strings.Group_Player,
        "live" => Strings.Group_Live,
        _ => Strings.Group_Other,
    };

    private void ApplyFilter()
    {
        MenuEntries.Clear();
        var filter = Filter?.Trim();
        foreach (var entry in _allEntries.Where(e =>
                     string.IsNullOrEmpty(filter) || e.Title.Contains(filter, StringComparison.CurrentCultureIgnoreCase)))
        {
            MenuEntries.Add(entry);
        }

        SelectedEntry = MenuEntries.FirstOrDefault();
    }
}
