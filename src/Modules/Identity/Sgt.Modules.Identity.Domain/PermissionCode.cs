using System.Text.RegularExpressions;
using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

/// <summary>
/// Bir ekran üzerindeki tek yetki: <c>&lt;ekran&gt;:&lt;eylem&gt;</c>.
/// Eylem "view" (görebilir), "operate" (işlem yapabilir) veya bir alt işlemdir (ör. "approve").
/// </summary>
public sealed partial record PermissionCode
{
    public const string ViewAction = "view";
    public const string OperateAction = "operate";

    private PermissionCode(string screen, string action)
    {
        Screen = screen;
        Action = action;
    }

    public string Screen { get; }

    public string Action { get; }

    public bool IsView => Action == ViewAction;

    public static Result<PermissionCode> Parse(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        var match = Format().Match(normalized);
        if (!match.Success)
        {
            return IdentityErrors.InvalidPermissionCode;
        }

        return new PermissionCode(match.Groups["screen"].Value, match.Groups["action"].Value);
    }

    public PermissionCode ViewOfScreen() => new(Screen, ViewAction);

    public override string ToString() => $"{Screen}:{Action}";

    [GeneratedRegex(@"^(?<screen>[a-z0-9_]+(\.[a-z0-9_]+)*):(?<action>[a-z0-9_]+)$")]
    private static partial Regex Format();
}
