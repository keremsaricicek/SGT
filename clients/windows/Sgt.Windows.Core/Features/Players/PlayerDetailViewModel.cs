using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Players;

/// <summary>Oyuncu kartı (O3), salt okunur. Belge numarası maskelidir.</summary>
public sealed partial class PlayerDetailViewModel(IPlayersApi players, Guid playerId) : StatusViewModel, IScreenViewModel
{
    public string Title => Player is null ? Strings.Screen_PlayerProfile : $"{Strings.Screen_PlayerProfile} — {Player.FirstName} {Player.LastName}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial PlayerDetailDto? Player { get; set; }

    public Task InitializeAsync(CancellationToken ct) => RefreshAsync(ct);

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var result = await players.GetAsync(playerId, ct);
            if (result.IsSuccess)
            {
                Player = result.Value;
                ClearMessages();
            }
            else
            {
                ShowProblem(result.Problem!);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
