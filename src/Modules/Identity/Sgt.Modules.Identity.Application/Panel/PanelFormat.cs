using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Panel;

internal static class PanelFormat
{
    public static IReadOnlyList<string> Codes(IEnumerable<PermissionCode> codes) =>
        codes.Select(c => c.ToString()).Order(StringComparer.Ordinal).ToList();
}
