using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>
/// Kasiyer vardiyası ekranı (K1): vardiya açma, mevcut vardiyaya bağlanma ve kapanış sayımı.
/// Açma ve kapama ayrı ViewModel'lerdir; bu sınıf yalnızca onları bir pencerede birleştirir.
/// </summary>
public sealed partial class CashierShiftViewModel : StatusViewModel, IScreenViewModel
{
    private readonly ICashierShiftContext _shift;

    public CashierShiftViewModel(OpenShiftViewModel open, CloseShiftViewModel close, ICashierShiftContext shift)
    {
        ArgumentNullException.ThrowIfNull(shift);
        Open = open;
        Close = close;
        _shift = shift;
        _shift.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrentShiftId));
            OnPropertyChanged(nameof(HasShift));
        };
    }

    public string Title => Strings.Screen_CageShift;

    public OpenShiftViewModel Open { get; }

    public CloseShiftViewModel Close { get; }

    public Guid? CurrentShiftId => _shift.ShiftId;

    public bool HasShift => _shift.ShiftId is not null;

    /// <summary>Uygulama yeniden başlatıldıysa açık vardiyanın numarası buraya girilerek bağlanılır.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AttachCommand))]
    public partial string? ShiftIdText { get; set; }

    public async Task InitializeAsync(CancellationToken ct)
    {
        if (HasShift)
        {
            await Close.LoadCommand.ExecuteAsync(null);
        }
    }

    private bool CanAttach() => Guid.TryParse(ShiftIdText?.Trim(), out _);

    [RelayCommand(CanExecute = nameof(CanAttach))]
    private async Task AttachAsync()
    {
        _shift.Attach(Guid.Parse(ShiftIdText!.Trim()));
        ShiftIdText = null;
        await Close.LoadCommand.ExecuteAsync(null);
        if (Close.ErrorMessage is not null)
        {
            _shift.Detach();
            ShowError(Close.ErrorMessage);
            return;
        }

        ShowStatus(Strings.Cage_ShiftAttached);
    }
}
