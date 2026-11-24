using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>Açılış çekmecesinde bir satır: araç (nakit/çip), para birimi, tutar.</summary>
public sealed partial class DrawerLineViewModel : ObservableObject
{
    public static IReadOnlyList<Instrument> Instruments { get; } = [Instrument.Cash, Instrument.Chips];

    public static IReadOnlyList<Currency> Currencies => Currency.All;

    [ObservableProperty]
    public partial Instrument Instrument { get; set; } = Instrument.Cash;

    [ObservableProperty]
    public partial Currency Currency { get; set; } = Currency.Try;

    [ObservableProperty]
    public partial string? AmountText { get; set; }

    /// <summary>Geçerli (sıfır veya pozitif) tutar; değilse null.</summary>
    public DrawerAmount? ToAmount() =>
        Amounts.TryParse(AmountText, CultureInfo.CurrentCulture, out var amount) && amount >= 0
            ? new DrawerAmount(Instrument, Currency.Code, amount)
            : null;
}

/// <summary>Kasiyer vardiyası açma (K1 "aç"): açılış çekmecesi sayılarak girilir.</summary>
public sealed partial class OpenShiftViewModel : StatusViewModel
{
    private readonly ICageShiftApi _api;
    private readonly ICashierShiftContext _shift;
    private readonly ISessionAccess _access;

    public OpenShiftViewModel(ICageShiftApi api, ICashierShiftContext shift, ISessionAccess access)
    {
        ArgumentNullException.ThrowIfNull(shift);
        _api = api;
        _shift = shift;
        _access = access;
        Lines.CollectionChanged += (_, e) =>
        {
            foreach (var line in e.NewItems?.OfType<DrawerLineViewModel>() ?? [])
            {
                line.PropertyChanged += OnLineChanged;
            }

            OpenCommand.NotifyCanExecuteChanged();
        };
        Lines.Add(new DrawerLineViewModel { Instrument = Instrument.Cash });
        Lines.Add(new DrawerLineViewModel { Instrument = Instrument.Chips });
        _shift.Changed += (_, _) => OpenCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<DrawerLineViewModel> Lines { get; } = [];

    public bool CanOpenShift => _access.Current.CanDo(ScreenCodes.CageShift, "open");

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    public partial string? ReasonCode { get; set; } = "SHIFT_OPEN";

    [RelayCommand]
    private void AddLine() => Lines.Add(new DrawerLineViewModel());

    [RelayCommand]
    private void RemoveLine(DrawerLineViewModel? line)
    {
        if (line is not null)
        {
            Lines.Remove(line);
        }
    }

    private bool CanOpen() =>
        CanOpenShift
        && _shift.ShiftId is null
        && Lines.Count > 0
        && Lines.All(l => l.ToAmount() is not null)
        && !string.IsNullOrWhiteSpace(ReasonCode);

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private async Task OpenAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var opening = Lines.Select(l => l.ToAmount()!).ToList();
            var result = await _api.OpenShiftAsync(new OpenShiftRequest(opening, ReasonCode!.Trim()), ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            _shift.Attach(result.Value.Id);
            ShowStatus(Strings.Cage_ShiftOpened);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnLineChanged(object? sender, PropertyChangedEventArgs e) => OpenCommand.NotifyCanExecuteChanged();
}
