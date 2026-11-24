namespace Sgt.Modules.Identity.Domain;

public readonly record struct RoleId(Guid Value)
{
    public static RoleId New() => new(Guid.NewGuid());
}
