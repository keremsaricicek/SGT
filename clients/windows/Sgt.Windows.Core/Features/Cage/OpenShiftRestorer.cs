using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>
/// Girişte kasiyerin sunucudaki açık vardiyasına otomatik bağlanır (GET /api/v1/cage/shifts/mine).
/// Vardiya ekranını görme yetkisi yoksa sunucuya sorulmaz; açık vardiya yoksa (204) bağlam boş kalır.
/// </summary>
internal sealed class OpenShiftRestorer(ICageShiftApi api, ICashierShiftContext shift) : ISessionInitializer
{
    public async Task<ApiProblem?> InitializeAsync(UserAccess access, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(access);
        if (!access.CanView(ScreenCodes.CageShift) || shift.ShiftId is not null)
        {
            return null;
        }

        var result = await api.GetMyOpenShiftAsync(ct);
        if (!result.IsSuccess)
        {
            return result.Problem;
        }

        if (result.Value is { } open)
        {
            shift.Attach(open.Id);
        }

        return null;
    }
}
