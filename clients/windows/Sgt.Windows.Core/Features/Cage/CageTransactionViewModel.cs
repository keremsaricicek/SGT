using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>
/// Oyuncuyla kasa işlemi ekranı: çip satış/bozdurma (K3) ve döviz (K4).
/// Her kullanıcı işlemi bir kez TransactionId alır; hata sonrası aynı girdilerle "tekrar" aynı kimlikle gider,
/// böylece sunucu ikinci bir para hareketi oluşturmaz. Girdi değişirse yeni işlem sayılır.
/// </summary>
public sealed partial class CageTransactionViewModel : StatusViewModel, IScreenViewModel
{
    private readonly ICageTransactionApi _api;
    private readonly ICashierShiftContext _shift;
    private readonly ISessionAccess _access;
    private readonly ICorrelationContext _correlation;
    private readonly PendingOperation _pending;

    public CageTransactionViewModel(
        string title,
        IReadOnlyList<ICageTransactionKind> kinds,
        ICageTransactionApi api,
        ICashierShiftContext shift,
        ISessionAccess access,
        ICorrelationContext correlation,
        IIdGenerator ids,
        PlayerPickerViewModel picker)
    {
        ArgumentNullException.ThrowIfNull(kinds);
        ArgumentNullException.ThrowIfNull(shift);
        ArgumentNullException.ThrowIfNull(picker);
        Title = title;
        _api = api;
        _shift = shift;
        _access = access;
        _correlation = correlation;
        _pending = new PendingOperation(ids);
        Kinds = kinds;
        Player = picker;
        Player.SelectionChanged += (_, _) => SubmitCommand.NotifyCanExecuteChanged();
        _shift.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(HasShift));
            SubmitCommand.NotifyCanExecuteChanged();
        };
        SelectedKind = kinds.FirstOrDefault(k => _access.Current.CanDo(k.ScreenCode, k.Action)) ?? kinds[0];
    }

    public string Title { get; }

    public IReadOnlyList<ICageTransactionKind> Kinds { get; }

    public PlayerPickerViewModel Player { get; }

    public bool HasShift => _shift.ShiftId is not null;

    /// <summary>Bekleyen (henüz başarıyla tamamlanmamış) işlemin kimliği; tekrar denemede aynı kalır.</summary>
    public Guid? PendingTransactionId => _pending.Current?.TransactionId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial ICageTransactionKind SelectedKind { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string? AmountText { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial Currency? SelectedCurrency { get; set; } = Currency.Try;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string? ReasonCode { get; set; }

    [ObservableProperty]
    public partial CageTransactionDto? LastTransaction { get; set; }

    public Task InitializeAsync(CancellationToken ct)
    {
        if (!HasShift)
        {
            ShowError(Strings.Cage_NoShift);
        }

        return Task.CompletedTask;
    }

    partial void OnSelectedKindChanged(ICageTransactionKind value) => ReasonCode = value?.DefaultReasonCode;

    private Money? ParsedAmount => Money.ParsePositive(AmountText, SelectedCurrency, CultureInfo.CurrentCulture);

    private bool CanSubmit() =>
        HasShift
        && Player.Selected is not null
        && ParsedAmount is not null
        && !string.IsNullOrWhiteSpace(ReasonCode)
        && _access.Current.CanDo(SelectedKind.ScreenCode, SelectedKind.Action);

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync(CancellationToken ct)
    {
        var shiftId = _shift.ShiftId!.Value;
        var money = ParsedAmount!;
        var input = new Input(SelectedKind, shiftId, Player.Selected!.PlayerId, money, ReasonCode!.Trim());
        var key = _pending.For(input);
        OnPropertyChanged(nameof(PendingTransactionId));

        IsBusy = true;
        try
        {
            ApiResult<CageTransactionDto> result;
            using (_correlation.Begin(key.CorrelationId))
            {
                var request = new CageRequest(key.TransactionId, input.PlayerId, money.Amount, money.Currency.Code, input.ReasonCode);
                result = await SelectedKind.ExecuteAsync(_api, shiftId, request, ct);
            }

            if (!result.IsSuccess)
            {
                // Bekleyen kimlik korunur: kullanıcı "tekrar" derse aynı işlem tekrar gönderilir.
                ShowProblem(result.Problem!);
                return;
            }

            _pending.Complete();
            OnPropertyChanged(nameof(PendingTransactionId));
            LastTransaction = result.Value;
            AmountText = null;
            ShowStatus(string.Format(CultureInfo.CurrentCulture, Strings.Cage_TransactionDone, SelectedKind.Title, money));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>İşlemi tanımlayan girdiler; değişirse yeni TransactionId üretilir.</summary>
    private sealed record Input(ICageTransactionKind Kind, Guid ShiftId, Guid PlayerId, Money Money, string ReasonCode);
}
