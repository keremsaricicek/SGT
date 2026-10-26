using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Players;

/// <summary>
/// Oyuncu arama ve seçme parçası. Oyuncu arama ekranında ve oyuncu gerektiren ekranlarda
/// (kasa işlemleri, rating) bileşen olarak kullanılır. En az <see cref="MinTermLength"/> karakter gerekir.
/// </summary>
public sealed partial class PlayerPickerViewModel(IPlayersApi players) : StatusViewModel
{
    /// <summary>Sunucudaki arama alt sınırıyla aynıdır (daha kısa terim boş sonuç döner).</summary>
    public const int MinTermLength = 2;

    public ObservableCollection<PlayerSummaryDto> Results { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TermTooShort))]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial string? Term { get; set; }

    [ObservableProperty]
    public partial PlayerSummaryDto? Selected { get; set; }

    public bool TermTooShort => (Term?.Trim().Length ?? 0) < MinTermLength;

    public event EventHandler? SelectionChanged;

    partial void OnSelectedChanged(PlayerSummaryDto? value) => SelectionChanged?.Invoke(this, EventArgs.Empty);

    private bool CanSearch() => !TermTooShort;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var result = await players.SearchAsync(Term!.Trim(), ct);
            Results.Clear();
            Selected = null;
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            foreach (var player in result.Value)
            {
                Results.Add(player);
            }

            if (Results.Count == 0)
            {
                ShowStatus(Strings.Players_NoResults);
            }
            else
            {
                ClearMessages();
                Selected = Results[0];
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
