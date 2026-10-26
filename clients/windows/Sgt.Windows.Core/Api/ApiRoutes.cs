using System.Globalization;

namespace Sgt.Windows.Core.Api;

/// <summary>
/// Bir API ucu: HTTP metodu ve sözleşmedeki yol şablonu (docs/api/openapi-v1.json ile birebir).
/// Sözleşme testi her rotanın OpenAPI dosyasında var olduğunu denetler.
/// </summary>
public sealed record ApiRoute(string Method, string Template)
{
    /// <summary>Şablondaki {parametre} alanlarını sırayla doldurur; taban adrese göre göreli yol döner.</summary>
    public Uri Build(params object[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var path = Template;
        foreach (var value in values)
        {
            var start = path.IndexOf('{', StringComparison.Ordinal);
            var end = path.IndexOf('}', StringComparison.Ordinal);
            if (start < 0 || end < start)
            {
                throw new ArgumentException("Şablonda doldurulacak parametre kalmadı.", nameof(values));
            }

            var text = Uri.EscapeDataString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
            path = string.Concat(path.AsSpan(0, start), text, path.AsSpan(end + 1));
        }

        if (path.Contains('{', StringComparison.Ordinal))
        {
            throw new ArgumentException("Şablondaki tüm parametreler doldurulmalıdır.", nameof(values));
        }

        return new Uri(path.TrimStart('/'), UriKind.Relative);
    }
}

/// <summary>Windows istemcisinin kullandığı API uçları (/api/v1).</summary>
public static class ApiRoutes
{
    public static readonly ApiRoute MyMenu = new("GET", "/api/v1/identity/me/menu");

    public static readonly ApiRoute OpenShift = new("POST", "/api/v1/cage/shifts");
    public static readonly ApiRoute GetShift = new("GET", "/api/v1/cage/shifts/{shiftId}");
    public static readonly ApiRoute ChipSales = new("POST", "/api/v1/cage/shifts/{shiftId}/chip-sales");
    public static readonly ApiRoute ChipRedemptions = new("POST", "/api/v1/cage/shifts/{shiftId}/chip-redemptions");
    public static readonly ApiRoute ExchangeForeignToBase = new("POST", "/api/v1/cage/shifts/{shiftId}/exchanges/foreign-to-base");
    public static readonly ApiRoute ExchangeBaseToForeign = new("POST", "/api/v1/cage/shifts/{shiftId}/exchanges/base-to-foreign");
    public static readonly ApiRoute CloseShift = new("POST", "/api/v1/cage/shifts/{shiftId}/close");
    public static readonly ApiRoute RequestAdjustment = new("POST", "/api/v1/cage/shifts/{shiftId}/adjustments");

    public static readonly ApiRoute SearchPlayers = new("GET", "/api/v1/players");
    public static readonly ApiRoute GetPlayer = new("GET", "/api/v1/players/{playerId}");

    public static readonly ApiRoute ListTables = new("GET", "/api/v1/floor/tables");
    public static readonly ApiRoute OpenTable = new("POST", "/api/v1/floor/tables/{tableId}/open");
    public static readonly ApiRoute CloseTable = new("POST", "/api/v1/floor/tables/{tableId}/close");
    public static readonly ApiRoute StartRating = new("POST", "/api/v1/floor/ratings");
    public static readonly ApiRoute FinishRating = new("POST", "/api/v1/floor/ratings/{ratingId}/finish");

    public static IReadOnlyList<ApiRoute> All { get; } =
    [
        MyMenu,
        OpenShift, GetShift, ChipSales, ChipRedemptions, ExchangeForeignToBase, ExchangeBaseToForeign, CloseShift, RequestAdjustment,
        SearchPlayers, GetPlayer,
        ListTables, OpenTable, CloseTable, StartRating, FinishRating,
    ];
}

/// <summary>Bu tipin API sözleşmesindeki (components/schemas) karşılığı. Sözleşme testi alanları karşılaştırır.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ApiSchemaAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
