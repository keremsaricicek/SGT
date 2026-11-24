using Microsoft.Extensions.DependencyInjection;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;
using Sgt.Modules.Identity.Infrastructure;

namespace Sgt.Modules.Identity.IntegrationTests;

public sealed class ScreenCatalogTests
{
    private static readonly IScreenCatalog Catalog = new ServiceCollection()
        .AddIdentityModule("Host=unused")
        .BuildServiceProvider()
        .GetRequiredService<IScreenCatalog>();

    [Fact]
    public void Catalog_has_the_38_listed_screens_with_unique_codes()
    {
        Assert.Equal(38, Catalog.All.Count);
        Assert.Equal(Catalog.All.Count, Catalog.All.Select(s => s.Code).Distinct().Count());
    }

    [Fact]
    public void Every_catalog_permission_is_a_valid_code()
    {
        var permissions = Catalog.All.SelectMany(s => s.AllPermissions()).ToList();

        Assert.All(permissions, p => Assert.True(Catalog.Defines(p)));
        Assert.DoesNotContain(Catalog.All, s => s.Actions.Contains("view"));
    }

    [Fact]
    public void Every_default_role_template_selects_at_least_one_screen_except_hotel_staff()
    {
        foreach (var template in DefaultRoleTemplates.All.Where(t => t.Name != DefaultRoleTemplates.HotelStaff))
        {
            Assert.Contains(Catalog.All, s => template.Includes(s));
        }
    }
}
