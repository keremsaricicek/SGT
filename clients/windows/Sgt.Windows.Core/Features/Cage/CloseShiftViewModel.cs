using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>Kapanış sayımında bir satır: beklenen tutar sunucudan gelir, sayılan tutar girilir, fark hesaplanır.</summary>
public sealed partial class CountLineViewModel(Instrument instrument, string currency, decimal expected, Action onChanged) : ObservableObject
{
    public Instrument Instrument { get; } = instrument;

    public string Currency { get; } = currency;

    public decimal Expected { get; } = expected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Counted), nameof(Variance), nameof(HasVariance))]
    public partial string? CountedText { get; set; }

    public decimal? Counted =>
        Amounts.TryParse(CountedText, CultureInfo.CurrentCulture, out var counted) && counted >= 0 ? counted : null;

    /// <summary>Sayılan − beklenen. Sayım girilmediyse null.</summary>
    public decimal? Variance => Counted - Expected;

    public bool HasVariance => Variance is { } v && v != 0;

    partial void OnCountedTextChanged(string? value) => onChanged();
}

/// <summary>
/// Kasiyer vardiyası kapanışı (K1 "kapat", "sayım"): her araç/para birimi sayılır.
/// Herhangi bir satırda fark varsa açıklama zorunludur (sunucu kuralı Finance.VarianceNoteRequired ile aynı).
/// </summary>
public sealed partial class CloseShiftViewModel : StatusViewModel
{
    private readonly ICageShiftApi _api;
    private readonly ICashierShiftContext _shift;
    private readonly ISessionAccess _access;

    public CloseShiftViewModel(ICageShiftApi api, ICashierShiftContext shift, ISessionAccess access)
    {
        ArgumentNullException.ThrowIfNull(shift);
        _api = api;
        _shift = shift;
        _access = access;
        _shift.Changed += (_, _) => CloseCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<CountLineViewModel> Lines { get; } = [];

    public bool CanCloseShift => _access.Current.CanDo(ScreenCodes.CageShift, "close");

    /// <summary>Sayımda fark var; açıklama girilmeden kapanış yapılamaz.</summary>
    public bool NoteRequired => Lines.Any(l => l.HasVariance);

    public decimal TotalVariance => Lines.Sum(l => l.Variance ?? 0);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CloseCommand))]
    [NotifyPropertyChangedFor(nameof(NoteMissing))]
    public partial string? Note { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CloseCommand))]
    public partial string? ReasonCode { get; set; } = "SHIFT_CLOSE";

    /// <summary>Ekranda uyarı göstermek için: fark var ama açıklama boş.</summary>
    public bool NoteMissing => NoteRequired && string.IsNullOrWhiteSpace(Note);

    /// <summary>Beklenen bakiyeleri sunucudan alır ve sayım satırlarını hazırlar.</summary>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        if (_shift.ShiftId is not { } shiftId)
        {
            ShowError(Strings.Cage_NoShift);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _api.GetShiftAsync(shiftId, ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            Lines.Clear();
            foreach (var balance in result.Value.Balances)
            {
                if (Enum.TryParse<Instrument>(balance.Instrument, ignoreCase: true, out var instrument))
                {
                    Lines.Add(new CountLineViewModel(instrument, balance.Currency, balance.Expected, OnCountChanged));
                }
            }

            OnCountChanged();
            ClearMessages();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanClose() =>
        CanCloseShift
        && _shift.ShiftId is not null
        && Lines.Count > 0
        && Lines.All(l => l.Counted is not null)
        && !string.IsNullOrWhiteSpace(ReasonCode)
        && !NoteMissing;

    [RelayCommand(CanExecute = nameof(CanClose))]
    private async Task CloseAsync(CancellationToken ct)
    {
        var shiftId = _shift.ShiftId!.Value;
        IsBusy = true;
        try
        {
            var counted = Lines.Select(l => new DrawerAmount(l.Instrument, l.Currency, l.Counted!.Value)).ToList();
            var note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();
            var result = await _api.CloseShiftAsync(shiftId, new CloseShiftRequest(counted, note, ReasonCode!.Trim()), ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            _shift.Detach();
            Lines.Clear();
            Note = null;
            OnCountChanged();
            ShowStatus(Strings.Cage_ShiftClosed);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCountChanged()
    {
        OnPropertyChanged(nameof(NoteRequired));
        OnPropertyChanged(nameof(NoteMissing));
        OnPropertyChanged(nameof(TotalVariance));
        CloseCommand.NotifyCanExecuteChanged();
    }
}
