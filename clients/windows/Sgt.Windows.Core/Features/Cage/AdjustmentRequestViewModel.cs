using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>
/// Düzeltme talebi (K5 "talep"): vardiyadaki bir işlem seçilir, açıklama yazılır. Kayıt silinmez/değiştirilmez;
/// ikinci bir yetkili onaylarsa sunucu ters kayıt oluşturur (dört göz).
/// </summary>
public sealed partial class AdjustmentRequestViewModel(
    ICageShiftApi shiftApi, ICageAdjustmentApi adjustmentApi, ICashierShiftContext shift, ISessionAccess access)
    : StatusViewModel, IScreenViewModel
{
    public string Title => Strings.Screen_CageAdjustment;

    public ObservableCollection<CageTransactionDto> Transactions { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial CageTransactionDto? SelectedTransaction { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string? Note { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string? ReasonCode { get; set; } = "CAGE_ADJUSTMENT";

    public Task InitializeAsync(CancellationToken ct) => LoadAsync(ct);

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        if (shift.ShiftId is not { } shiftId)
        {
            ShowError(Strings.Cage_NoShift);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await shiftApi.GetShiftAsync(shiftId, ct);
            Transactions.Clear();
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            foreach (var transaction in result.Value.Transactions)
            {
                Transactions.Add(transaction);
            }

            ClearMessages();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSubmit() =>
        shift.ShiftId is not null
        && SelectedTransaction is not null
        && !string.IsNullOrWhiteSpace(Note)
        && !string.IsNullOrWhiteSpace(ReasonCode)
        && access.Current.CanDo(ScreenCodes.CageAdjustment, "request");

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var request = new AdjustmentRequest(SelectedTransaction!.TransactionId, Note!.Trim(), ReasonCode!.Trim());
            var result = await adjustmentApi.RequestAdjustmentAsync(shift.ShiftId!.Value, request, ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            Note = null;
            SelectedTransaction = null;
            ShowStatus(Strings.Cage_AdjustmentRequested);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
