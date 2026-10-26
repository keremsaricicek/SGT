using System.Reflection;
using System.Runtime.CompilerServices;
using NetArchTest.Rules;
using Xunit;

namespace Sgt.Tests.Architecture;

// K1 (SOLID/OOP) ve K2 (işlev ayrımı + sözleşmeli entegrasyon) kurallarını her build'de denetler.
// Gerekli paketler: xunit, NetArchTest.Rules. Ayrıntı: docs/kod-kurallari.md bölüm 6.
// Yeni bir modül eklendiğinde yalnızca ModuleCatalog'a bir satır eklenir.
public sealed class ArchitectureTests
{
    private static readonly string[] FrameworkNamespaces =
    {
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql",
        "Confluent.Kafka",
        "RabbitMQ.Client",
    };

    [Fact]
    public void BuildingBlocks_do_not_depend_on_frameworks_or_modules()
    {
        var result = Types.InAssembly(typeof(Sgt.BuildingBlocks.Money).Assembly)
            .ShouldNot().HaveDependencyOnAny(FrameworkNamespaces.Append("Sgt.Modules").ToArray())
            .GetResult();

        AssertNone(Describe(result, "K1/DIP: BuildingBlocks altyapı ve modül bilmez").ToList());
    }

    [Fact]
    public void Modules_reference_each_other_only_through_contracts()
    {
        var violations = new List<string>();

        foreach (var module in ModuleCatalog.All)
        foreach (var other in ModuleCatalog.All.Where(m => m.Name != module.Name))
        {
            var result = Types.InAssemblies(module.NonContractAssemblies)
                .That().ResideInNamespace(module.RootNamespace)
                .ShouldNot().HaveDependencyOnAny(other.InternalNamespaces)
                .GetResult();

            violations.AddRange(Describe(result,
                $"K2: {module.Name} → {other.Name} iç koduna erişemez; yalnızca {other.Name}.Contracts kullanılır"));
        }

        AssertNone(violations);
    }

    [Fact]
    public void Domain_does_not_depend_on_application_infrastructure_or_frameworks()
    {
        var violations = new List<string>();

        foreach (var module in ModuleCatalog.All)
        {
            var forbidden = FrameworkNamespaces
                .Append($"{module.RootNamespace}.Application")
                .Append($"{module.RootNamespace}.Infrastructure")
                .Append($"{module.RootNamespace}.Api")
                .ToArray();

            var result = Types.InAssembly(module.Domain)
                .That().ResideInNamespace(module.RootNamespace)
                .ShouldNot().HaveDependencyOnAny(forbidden)
                .GetResult();

            violations.AddRange(Describe(result, $"K1/DIP: {module.Name}.Domain altyapı ve üst katmanları bilmez"));
        }

        AssertNone(violations);
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_frameworks()
    {
        var violations = new List<string>();

        foreach (var module in ModuleCatalog.All)
        {
            var forbidden = FrameworkNamespaces
                .Append($"{module.RootNamespace}.Infrastructure")
                .Append($"{module.RootNamespace}.Api")
                .ToArray();

            var result = Types.InAssembly(module.Application)
                .That().ResideInNamespace(module.RootNamespace)
                .ShouldNot().HaveDependencyOnAny(forbidden)
                .GetResult();

            violations.AddRange(Describe(result, $"K1/DIP: {module.Name}.Application yalnızca soyutlamalara bağlıdır"));
        }

        AssertNone(violations);
    }

    [Fact]
    public void Contracts_are_pure()
    {
        var violations = new List<string>();
        var allInternal = ModuleCatalog.All.SelectMany(m => m.InternalNamespaces).ToArray();

        foreach (var module in ModuleCatalog.All)
        {
            var result = Types.InAssembly(module.Contracts)
                .That().ResideInNamespace(module.RootNamespace)
                .ShouldNot().HaveDependencyOnAny(allInternal.Concat(FrameworkNamespaces).ToArray())
                .GetResult();

            violations.AddRange(Describe(result, $"K2: {module.Name}.Contracts hiçbir modülün iç koduna bağlı olamaz"));
        }

        AssertNone(violations);
    }

    [Fact]
    public void Domain_types_do_not_expose_public_setters()
    {
        var violations = ModuleCatalog.All
            .SelectMany(m => m.Domain.GetTypes())
            .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .SelectMany(t => t
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(p => p.SetMethod is { IsPublic: true } && !IsInitOnly(p))
                .Select(p => $"[K1/kapsülleme] {t.FullName}.{p.Name}: public set yasak; durumu davranış metoduyla değiştirin"))
            .ToList();

        AssertNone(violations);
    }

    private static bool IsInitOnly(PropertyInfo property) =>
        property.SetMethod!.ReturnParameter
            .GetRequiredCustomModifiers()
            .Contains(typeof(IsExternalInit));

    private static IEnumerable<string> Describe(TestResult result, string rule) =>
        result.IsSuccessful
            ? Enumerable.Empty<string>()
            : (result.FailingTypeNames ?? Array.Empty<string>()).Select(type => $"[{rule}] {type}");

    private static void AssertNone(List<string> violations) =>
        Assert.True(violations.Count == 0,
            "Kod kuralı ihlali:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
}

public sealed record ModuleAssemblies(
    string Name,
    Assembly Contracts,
    Assembly Domain,
    Assembly Application,
    Assembly Infrastructure,
    Assembly Api)
{
    public string RootNamespace => $"Sgt.Modules.{Name}";

    public Assembly[] NonContractAssemblies => new[] { Domain, Application, Infrastructure, Api };

    public string[] InternalNamespaces => new[]
    {
        $"{RootNamespace}.Domain",
        $"{RootNamespace}.Application",
        $"{RootNamespace}.Infrastructure",
        $"{RootNamespace}.Api",
    };
}

internal static class ModuleCatalog
{
    // Her modül projesinde boş bir "AssemblyMarker" sınıfı bulunur. Modül eklendikçe satır eklenir:
    //
    // new("Hotel",
    //     typeof(Sgt.Modules.Hotel.Contracts.AssemblyMarker).Assembly,
    //     typeof(Sgt.Modules.Hotel.Domain.AssemblyMarker).Assembly,
    //     typeof(Sgt.Modules.Hotel.Application.AssemblyMarker).Assembly,
    //     typeof(Sgt.Modules.Hotel.Infrastructure.AssemblyMarker).Assembly,
    //     typeof(Sgt.Modules.Hotel.Api.AssemblyMarker).Assembly),
    public static readonly IReadOnlyList<ModuleAssemblies> All =
    [
    ];
}
