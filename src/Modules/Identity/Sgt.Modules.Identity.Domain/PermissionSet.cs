using System.Collections.Immutable;
using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

/// <summary>
/// Verilmiş yetkiler kümesi. Değişmezdir; her değişiklik yeni küme döndürür.
/// Kural: bir ekranın görme dışındaki yetkisi ancak görme yetkisiyle birlikte bulunabilir.
/// </summary>
public sealed class PermissionSet
{
    public static readonly PermissionSet Empty = new(ImmutableHashSet<PermissionCode>.Empty);

    private readonly ImmutableHashSet<PermissionCode> _codes;

    private PermissionSet(ImmutableHashSet<PermissionCode> codes)
    {
        _codes = codes;
    }

    public IReadOnlySet<PermissionCode> Codes => _codes;

    public bool Contains(PermissionCode code) => _codes.Contains(code);

    public Result<PermissionSet> With(PermissionCode code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (!code.IsView && !_codes.Contains(code.ViewOfScreen()))
        {
            return IdentityErrors.ViewRequired;
        }

        return new PermissionSet(_codes.Add(code));
    }

    public Result<PermissionSet> Without(PermissionCode code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (code.IsView && _codes.Any(c => c.Screen == code.Screen && !c.IsView))
        {
            return IdentityErrors.ViewInUse;
        }

        return new PermissionSet(_codes.Remove(code));
    }
}
