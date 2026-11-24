using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests;

/// <summary>Testler için yardımcılar: yetki, kimlik üretici ve vardiya bağlamı.</summary>
internal static class TestSupport
{
    public static ISessionAccess Access(params (string Screen, string[] Actions)[] screens)
    {
        var access = new SessionAccess();
        access.Set(UserAccess.From(screens.Select(s => new MenuItemDto(s.Screen, s.Actions))));
        return access;
    }

    public static ICashierShiftContext ShiftAttached(Guid shiftId)
    {
        var shift = new CashierShiftContext();
        shift.Attach(shiftId);
        return shift;
    }

    public static ApiProblem Problem(int status, string code, string detail) => new(status, code, code, detail);
}

/// <summary>Sırayla 1, 2, 3... ile biten öngörülebilir kimlikler üretir.</summary>
internal sealed class SequentialIds : IIdGenerator
{
    private int _next;

    public Guid NewId() => new($"00000000-0000-0000-0000-{++_next:D12}");
}
