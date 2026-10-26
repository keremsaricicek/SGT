using Microsoft.Extensions.DependencyInjection;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>Kasa özelliğinin servis ve ekran kayıtları.</summary>
public static class CageFeature
{
    public static IServiceCollection AddCageFeature(this IServiceCollection services)
    {
        services.AddTransient<CageApiClient>();
        services.AddTransient<ICageShiftApi>(sp => sp.GetRequiredService<CageApiClient>());
        services.AddTransient<ICageTransactionApi>(sp => sp.GetRequiredService<CageApiClient>());
        services.AddTransient<ICageAdjustmentApi>(sp => sp.GetRequiredService<CageApiClient>());

        services.AddTransient<ISessionInitializer, OpenShiftRestorer>();
        services.AddTransient<OpenShiftViewModel>();
        services.AddTransient<CloseShiftViewModel>();
        services.AddScreen<CashierShiftViewModel>(ScreenCodes.CageShift, () => Strings.Screen_CageShift);
        services.AddScreen<ShiftSummaryViewModel>(ScreenCodes.CageSummary, () => Strings.Screen_CageSummary);
        services.AddScreen<AdjustmentRequestViewModel>(ScreenCodes.CageAdjustment, () => Strings.Screen_CageAdjustment);

        services.AddSingleton(new ScreenDescriptor(
            ScreenCodes.CageChips,
            () => Strings.Screen_CageChips,
            (sp, _) => Transaction(sp, Strings.Screen_CageChips, [new SellChips(), new RedeemChips()])));
        services.AddSingleton(new ScreenDescriptor(
            ScreenCodes.CageExchange,
            () => Strings.Screen_CageExchange,
            (sp, _) => Transaction(sp, Strings.Screen_CageExchange, [new ExchangeForeignToBase(), new ExchangeBaseToForeign()])));
        return services;
    }

    private static CageTransactionViewModel Transaction(IServiceProvider sp, string title, IReadOnlyList<ICageTransactionKind> kinds) =>
        new(
            title,
            kinds,
            sp.GetRequiredService<ICageTransactionApi>(),
            sp.GetRequiredService<ICashierShiftContext>(),
            sp.GetRequiredService<ISessionAccess>(),
            sp.GetRequiredService<ICorrelationContext>(),
            sp.GetRequiredService<IIdGenerator>(),
            sp.GetRequiredService<PlayerPickerViewModel>());
}
