import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:material_ui/material_ui.dart';

import '../access/screen_codes.dart';
import '../l10n/gen/app_localizations.dart';

enum MenuArea { player, loyalty, live, slot, reports }

/// Mobilde karşılığı olan ekranlar (KK3). Sunucu menüsünde olmayan ekran listelenmez.
class MobileScreen {
  const MobileScreen(this.code, this.area, this.route, this.icon, this._title);

  final String code;
  final MenuArea area;
  final String route;
  final IconData icon;
  final String Function(AppLocalizations) _title;

  String title(AppLocalizations l) => _title(l);

  static final all = <MobileScreen>[
    MobileScreen(ScreenCodes.playerSearch, MenuArea.player, '/players', FluentIcons.search_24_regular, (l) => l.screenPlayerSearch),
    MobileScreen(ScreenCodes.playerLocator, MenuArea.player, '/locator', FluentIcons.location_24_regular, (l) => l.screenPlayerLocator),
    MobileScreen(ScreenCodes.loyaltyPoints, MenuArea.loyalty, '/loyalty', FluentIcons.star_24_regular, (l) => l.screenLoyaltyPoints),
    MobileScreen(ScreenCodes.loyaltyComp, MenuArea.loyalty, '/comp', FluentIcons.gift_24_regular, (l) => l.screenCompRequest),
    MobileScreen(ScreenCodes.liveTables, MenuArea.live, '/floor/tables', FluentIcons.table_24_regular, (l) => l.screenTables),
    MobileScreen(ScreenCodes.slotFloor, MenuArea.slot, '/floor/slots', FluentIcons.grid_24_regular, (l) => l.screenSlots),
    MobileScreen(ScreenCodes.reportDaily, MenuArea.reports, '/reports/daily', FluentIcons.data_bar_vertical_24_regular, (l) => l.screenDailySummary),
  ];

  static String areaTitle(AppLocalizations l, MenuArea a) => switch (a) {
        MenuArea.player => l.areaPlayer,
        MenuArea.loyalty => l.areaLoyalty,
        MenuArea.live => l.areaLive,
        MenuArea.slot => l.areaSlot,
        MenuArea.reports => l.areaReports,
      };
}
