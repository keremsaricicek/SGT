using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Features.Players;

/// <summary>Oyuncu arama ekranı (O1). Seçili oyuncunun kartı yetki varsa ayrı pencerede açılır (O3).</summary>
public sealed partial class PlayerSearchViewModel : Shared.IScreenViewModel
{
    private readonly IScreenOpener _opener;
    private readonly ISessionAccess _access;

    public PlayerSearchViewModel(PlayerPickerViewModel picker, IScreenOpener opener, ISessionAccess access)
    {
        ArgumentNullException.ThrowIfNull(picker);
        Picker = picker;
        _opener = opener;
        _access = access;
        Picker.SelectionChanged += (_, _) => OpenDetailCommand.NotifyCanExecuteChanged();
    }

    public string Title => Strings.Screen_PlayerSearch;

    public PlayerPickerViewModel Picker { get; }

    public bool CanOpenProfile => _access.Current.CanView(ScreenCodes.PlayerProfile);

    public Task InitializeAsync(CancellationToken ct) => Task.CompletedTask;

    private bool CanOpenDetail() => Picker.Selected is not null && CanOpenProfile;

    [RelayCommand(CanExecute = nameof(CanOpenDetail))]
    private async Task OpenDetailAsync() => await
        _opener.OpenAsync(new ScreenRequest(ScreenCodes.PlayerProfile, Picker.Selected!.PlayerId), CancellationToken.None);
}
