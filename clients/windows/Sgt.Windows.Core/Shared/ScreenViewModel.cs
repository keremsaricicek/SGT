using CommunityToolkit.Mvvm.ComponentModel;
using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Shared;

/// <summary>Pencerede açılan her ekranın ortak sözleşmesi.</summary>
public interface IScreenViewModel
{
    string Title { get; }

    /// <summary>Pencere açıldıktan sonra bir kez çağrılır (ilk verinin yüklenmesi).</summary>
    Task InitializeAsync(CancellationToken ct);
}

/// <summary>Ekranların ortak durum alanları: meşgul, hata (problem details) ve bilgi mesajı.</summary>
public abstract partial class StatusViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    /// <summary>Sunucudan dönen hatanın kodu (ör. Finance.VarianceNoteRequired); destek için ekranda gösterilir.</summary>
    [ObservableProperty]
    public partial string? ErrorCode { get; set; }

    protected void ShowProblem(ApiProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        StatusMessage = null;
        ErrorMessage = problem.Message;
        ErrorCode = problem.Code;
    }

    protected void ShowError(string message)
    {
        StatusMessage = null;
        ErrorMessage = message;
        ErrorCode = null;
    }

    protected void ShowStatus(string message)
    {
        ErrorMessage = null;
        ErrorCode = null;
        StatusMessage = message;
    }

    protected void ClearMessages()
    {
        ErrorMessage = null;
        ErrorCode = null;
        StatusMessage = null;
    }
}
