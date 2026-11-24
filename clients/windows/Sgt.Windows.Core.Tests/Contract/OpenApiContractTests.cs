using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Tests.Contract;

/// <summary>
/// İstemcinin kullandığı rotalar, istek ve yanıt DTO'ları API sözleşmesiyle (docs/api/openapi-v1.json, ADR-0006) aynı olmalıdır.
/// Sözleşme değişirse bu testler kırmızıya döner; istemci güncellenmeden birleşme yapılmaz.
/// </summary>
public sealed class OpenApiContractTests
{
    private static readonly JsonNode Document = JsonNode.Parse(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contract", "openapi-v1.json")))!;

    public static TheoryData<string, string> Routes()
    {
        var data = new TheoryData<string, string>();
        foreach (var route in ApiRoutes.All)
        {
            data.Add(route.Method, route.Template);
        }

        return data;
    }

    public static TheoryData<string> SchemaTypes()
    {
        var data = new TheoryData<string>();
        foreach (var type in SchemaBoundTypes())
        {
            data.Add(type.FullName!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public void Rota_sozlesmede_vardir(string method, string template)
    {
        var path = Document["paths"]?[template];
        Assert.True(path is not null, $"Sözleşmede yol yok: {template}");
        Assert.True(path![method.ToLowerInvariant()] is not null, $"Sözleşmede {method} {template} yok.");
    }

    [Theory]
    [MemberData(nameof(SchemaTypes))]
    public void Istek_DTOsu_sozlesmedeki_semayla_ayni_alanlara_sahiptir(string typeName)
    {
        var type = SchemaBoundTypes().Single(t => t.FullName == typeName);
        var schemaName = type.GetCustomAttribute<ApiSchemaAttribute>()!.Name;
        var schema = Document["components"]?["schemas"]?[schemaName];
        Assert.True(schema is not null, $"Sözleşmede şema yok: {schemaName}");

        var expected = schema!["properties"]!.AsObject().Select(p => p.Key).Order(StringComparer.Ordinal).ToList();
        var required = schema["required"]?.AsArray().Select(r => r!.GetValue<string>()).Order(StringComparer.Ordinal).ToList() ?? [];
        // Sözleşme alanları kayıt (record) yapıcısının parametreleridir; hesaplanan ekran özellikleri (ör. FullName) sayılmaz.
        var constructor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var actual = constructor.GetParameters()
            .Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name!))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(expected, actual);
        Assert.Equal(required, actual);
    }

    [Fact]
    public void Tum_istek_semalari_icin_en_az_bir_DTO_vardir() =>
        Assert.Contains(SchemaBoundTypes(), t => t.GetCustomAttribute<ApiSchemaAttribute>()!.Name == "CageRequest");

    private static IEnumerable<Type> SchemaBoundTypes() =>
        typeof(ApiRoutes).Assembly.GetTypes().Where(t => t.GetCustomAttribute<ApiSchemaAttribute>() is not null);
}
