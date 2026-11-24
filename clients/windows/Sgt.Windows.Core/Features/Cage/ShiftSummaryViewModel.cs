using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>Kasa özeti (K6): beklenen bakiyeler, vardiyadaki işlemler ve varsa kapanış sayımı. Salt okunur.</summary>
public sealed partial class ShiftSummaryViewModel(ICageShiftApi api, ICashierShiftContext shift) : StatusViewModel, IScreenViewModel
{
    public string Title => Strings.Screen_CageSummary;

    [ObservableProperty]
    public partial ShiftSummaryDto? Summary { get; set; }

    public Task InitializeAsync(CancellationToken ct) => RefreshAsync(ct);

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        if (shift.ShiftId is not { } shiftId)
        {
            Summary = null;
            ShowError(Strings.Cage_NoShift);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await api.GetShiftAsync(shiftId, ct);
            if (result.IsSuccess)
            {
                Summary = result.Value;
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
