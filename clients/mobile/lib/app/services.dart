import 'package:material_ui/material_ui.dart';

import '../access/access_controller.dart';
import '../api/generated/sgt_api.g.dart';
import '../auth/auth_service.dart';
import '../core/loyalty_points.dart';
import '../features/floor/data/api_floor_repository.dart';
import '../features/floor/domain/floor_repository.dart';
import '../features/loyalty/data/api_loyalty_repository.dart';
import '../features/loyalty/domain/loyalty_repository.dart';
import '../features/players/data/api_players_repository.dart';
import '../features/players/domain/players_repository.dart';
import '../features/reports/data/api_reports_repository.dart';
import '../features/reports/domain/reports_repository.dart';

/// Uygulama servisleri (bileşim kökü). Özellikler birbirini bilmez; ortak sözleşmeler (`lib/core`) burada
/// uygulamalarına bağlanır. Ekranlar `AppServices.of(context)` ile alır; testte sahte taşıma verilir.
class AppServices {
  AppServices({
    required this.auth,
    required this.api,
    required this.terminalId,
    required this.newId,
  })  : access = AccessController(api),
        players = ApiPlayersRepository(api),
        loyalty = ApiLoyaltyRepository(api),
        reports = ApiReportsRepository(api),
        floor = ApiFloorRepository(api);

  final AuthService auth;
  final SgtApiClient api;
  final AccessController access;
  final String terminalId;

  /// İstemci kaynaklı idempotency anahtarı (UUID v4).
  final String Function() newId;

  final PlayersRepository players;
  final LoyaltyRepository loyalty;
  final ReportsRepository reports;
  final FloorRepository floor;

  /// Oyuncu ekranının kullandığı puan okuma sözleşmesi (loyalty uygulaması).
  LoyaltyPointsReader get loyaltyPoints => loyalty;

  static AppServices of(BuildContext context) =>
      context.dependOnInheritedWidgetOfExactType<ServicesScope>()!.services;
}

class ServicesScope extends InheritedWidget {
  const ServicesScope({super.key, required this.services, required super.child});

  final AppServices services;

  @override
  bool updateShouldNotify(ServicesScope oldWidget) => services != oldWidget.services;
}
