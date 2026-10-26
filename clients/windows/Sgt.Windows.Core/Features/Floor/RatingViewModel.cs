using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Floor;

/// <summary>Bu terminalden başlatılmış, henüz bitirilmemiş rating.</summary>
public sealed record ActiveRating(Guid RatingId, string TableCode, string PlayerName, DateTimeOffset StartedAt);

/// <summary>
/// Oyuncu derecelendirme (C3): açık masada oyuncu için rating başlatılır; bitirirken ortalama bahis
/// ve (biliniyorsa) oyuncu sonucu girilir.
/// </summary>
public sealed partial class RatingViewModel : StatusViewModel, IScreenViewModel
{
    private readonly ITablesApi _tables;
    private readonly IRatingsApi _ratings;
    private readonly ISessionAccess _access;
    private readonly TimeProvider _clock;

    public RatingViewModel(ITablesApi tables, IRatingsApi ratings, ISessionAccess access, TimeProvider clock, PlayerPickerViewModel picker)
    {
        ArgumentNullException.ThrowIfNull(picker);
        _tables = tables;
        _ratings = ratings;
        _access = access;
        _clock = clock;
        Player = picker;
        Player.SelectionChanged += (_, _) => StartCommand.NotifyCanExecuteChanged();
    }

    public string Title => Strings.Screen_Rating;

    public PlayerPickerViewModel Player { get; }

    public ObservableCollection<TableDto> OpenTables { get; } = [];

    public ObservableCollection<ActiveRating> ActiveRatings { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    public partial TableDto? SelectedTable { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    public partial ActiveRating? SelectedRating { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    public partial string? AverageBetText { get; set; }

    /// <summary>Oyuncu sonucu (kazanç +, kayıp −). Boş bırakılabilir.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    public partial string? PlayerResultText { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(FinishCommand))]
    public partial string? ReasonCode { get; set; } = "RATING";

    public Task InitializeAsync(CancellationToken ct) => LoadTablesAsync(ct);

    [RelayCommand]
    private async Task LoadTablesAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var result = await _tables.ListTablesAsync(ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            OpenTables.Clear();
            foreach (var table in result.Value.Where(t => t.IsOpen))
            {
                OpenTables.Add(table);
            }

            SelectedTable ??= OpenTables.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanStart() =>
        _access.Current.CanDo(ScreenCodes.LiveRating, "start")
        && SelectedTable is not null
        && Player.Selected is not null
        && !string.IsNullOrWhiteSpace(ReasonCode);

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync(CancellationToken ct)
    {
        var table = SelectedTable!;
        var player = Player.Selected!;
        IsBusy = true;
        try
        {
            var result = await _ratings.StartRatingAsync(new StartRatingRequest(table.TableId, player.PlayerId, ReasonCode!.Trim()), ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            var rating = new ActiveRating(result.Value.Id, table.Code, player.FullName, _clock.GetLocalNow());
            ActiveRatings.Add(rating);
            SelectedRating = rating;
            ShowStatus(string.Format(CultureInfo.CurrentCulture, Strings.Floor_RatingStarted, player.FullName, table.Code));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private decimal? AverageBet =>
        Amounts.TryParse(AverageBetText, CultureInfo.CurrentCulture, out var bet) && bet >= 0 ? bet : null;

    private bool PlayerResultValid =>
        string.IsNullOrWhiteSpace(PlayerResultText) || Amounts.TryParse(PlayerResultText, CultureInfo.CurrentCulture, out _);

    private bool CanFinish() =>
        _access.Current.CanDo(ScreenCodes.LiveRating, "finish")
        && SelectedRating is not null
        && AverageBet is not null
        && PlayerResultValid
        && !string.IsNullOrWhiteSpace(ReasonCode);

    [RelayCommand(CanExecute = nameof(CanFinish))]
    private async Task FinishAsync(CancellationToken ct)
    {
        var rating = SelectedRating!;
        decimal? playerResult = Amounts.TryParse(PlayerResultText, CultureInfo.CurrentCulture, out var value) ? value : null;
        IsBusy = true;
        try
        {
            var result = await _ratings.FinishRatingAsync(
                rating.RatingId, new RatingValuesRequest(AverageBet!.Value, playerResult, ReasonCode!.Trim()), ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            ActiveRatings.Remove(rating);
            SelectedRating = ActiveRatings.FirstOrDefault();
            AverageBetText = null;
            PlayerResultText = null;
            ShowStatus(string.Format(CultureInfo.CurrentCulture, Strings.Floor_RatingFinished, rating.PlayerName, rating.TableCode));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
