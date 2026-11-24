using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Floor;

/// <summary>Masa listesi / pit görünümü (C1) ve masa aç/kapat (C2). Aç/kapat ayrı ekran yetkisidir (live.table_session).</summary>
public sealed partial class TablesViewModel(ITablesApi api, ISessionAccess access) : StatusViewModel, IScreenViewModel
{
    public string Title => Strings.Screen_Tables;

    public ObservableCollection<TableDto> Tables { get; } = [];

    public bool CanOpenTables => access.Current.CanDo(ScreenCodes.LiveTableSession, "open");

    public bool CanCloseTables => access.Current.CanDo(ScreenCodes.LiveTableSession, "close");

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenTableCommand), nameof(CloseTableCommand))]
    public partial TableDto? SelectedTable { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenTableCommand), nameof(CloseTableCommand))]
    public partial string? ReasonCode { get; set; } = "TABLE_SESSION";

    public Task InitializeAsync(CancellationToken ct) => RefreshAsync(ct);

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var selected = SelectedTable?.TableId;
            var result = await api.ListTablesAsync(ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }

            Tables.Clear();
            foreach (var table in result.Value)
            {
                Tables.Add(table);
            }

            SelectedTable = Tables.FirstOrDefault(t => t.TableId == selected) ?? Tables.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanOpenTable() => CanOpenTables && SelectedTable is { IsOpen: false } && HasReason;

    private bool CanCloseTable() => CanCloseTables && SelectedTable is { IsOpen: true } && HasReason;

    private bool HasReason => !string.IsNullOrWhiteSpace(ReasonCode);

    [RelayCommand(CanExecute = nameof(CanOpenTable))]
    private Task OpenTableAsync(CancellationToken ct) =>
        ChangeSession(SelectedTable!, api.OpenTableAsync, Strings.Floor_TableOpened, ct);

    [RelayCommand(CanExecute = nameof(CanCloseTable))]
    private Task CloseTableAsync(CancellationToken ct) =>
        ChangeSession(SelectedTable!, api.CloseTableAsync, Strings.Floor_TableClosed, ct);

    private async Task ChangeSession(
        TableDto table, Func<Guid, FloorReasonRequest, CancellationToken, Task<ApiResult<Done>>> call, string doneFormat, CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var result = await call(table.TableId, new FloorReasonRequest(ReasonCode!.Trim()), ct);
            if (!result.IsSuccess)
            {
                ShowProblem(result.Problem!);
                return;
            }
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync(ct);
        ShowStatus(string.Format(System.Globalization.CultureInfo.CurrentCulture, doneFormat, table.Code));
    }
}
