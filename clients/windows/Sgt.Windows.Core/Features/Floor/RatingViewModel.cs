using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Floor;

/// <summary>
/// Süren rating satırı. Oyuncu adı yalnızca bu ekrandan başlatılan rating'lerde bilinir;
/// diğerlerinde oyuncu kimliğinin kısa hali gösterilir (sunucu listesi ad taşımaz).
/// </summary>
public sealed record ActiveRating(Guid RatingId, Guid PlayerId, string TableCode, string PlayerName, DateTimeOffset StartedAt);

/// <summary>
/// Oyuncu derecelendirme (C3): açık masada oyuncu için rating başlatılır; süren rating'ler sunucudan gelir
/// (GET /api/v1/floor/ratings/running), bitirirken ortalama bahis ve (biliniyorsa) oyuncu sonucu girilir.
/// </summary>
public sealed partial class RatingViewModel : StatusViewModel, IScreenViewModel
{
    private readonly ITablesApi _tables;
    private readonly IRatingsApi _ratings;
    private readonly ISessionAccess _access;
    private readonly Dictionary<Guid, string> _knownPlayers = [];

    public RatingViewModel(ITablesApi tables, IRatingsApi ratings, ISessionAccess access, PlayerPickerViewModel picker)
    {
        ArgumentNullException.ThrowIfNull(picker);
        _tables = tables;
        _ratings = ratings;
        _access = access;
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

    public Task InitializeAsync(CancellationToken ct) => LoadAsync(ct);

    /// <summary>Açık masaları ve süren rating'leri sunucudan yükler (F5).</summary>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var tables = await _tables.ListTablesAsync(ct);
            if (!tables.IsSuccess)
            {
                ShowProblem(tables.Problem!);
                return;
            }

            OpenTables.Clear();
            foreach (var table in tables.Value.Where(t => t.IsOpen))
            {
                OpenTables.Add(table);
            }

            SelectedTable = OpenTables.FirstOrDefault(t => t.TableId == SelectedTable?.TableId) ?? OpenTables.FirstOrDefault();
            await LoadRunningAsync(null, ct);
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

            _knownPlayers[player.PlayerId] = player.FullName;
            if (await LoadRunningAsync(result.Value.Id, ct))
            {
                ShowStatus(string.Format(CultureInfo.CurrentCulture, Strings.Floor_RatingStarted, player.FullName, table.Code));
            }
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

            AverageBetText = null;
            PlayerResultText = null;
            if (await LoadRunningAsync(null, ct))
            {
                ShowStatus(string.Format(CultureInfo.CurrentCulture, Strings.Floor_RatingFinished, rating.PlayerName, rating.TableCode));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Süren rating listesini yeniler; <paramref name="select"/> verilirse o satır seçilir.</summary>
    private async Task<bool> LoadRunningAsync(Guid? select, CancellationToken ct)
    {
        var running = await _ratings.ListRunningAsync(null, ct);
        if (!running.IsSuccess)
        {
            ShowProblem(running.Problem!);
            return false;
        }

        var previous = select ?? SelectedRating?.RatingId;
        ActiveRatings.Clear();
        foreach (var rating in running.Value.OrderBy(r => r.StartedAt))
        {
            var name = _knownPlayers.TryGetValue(rating.PlayerId, out var known) ? known : $"#{rating.PlayerId.ToString("N")[..8]}";
            ActiveRatings.Add(new ActiveRating(rating.RatingId, rating.PlayerId, rating.TableCode, name, rating.StartedAt));
        }

        SelectedRating = ActiveRatings.FirstOrDefault(r => r.RatingId == previous) ?? ActiveRatings.FirstOrDefault();
        ClearMessages();
        return true;
    }
}
