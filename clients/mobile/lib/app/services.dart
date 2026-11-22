import 'package:material_ui/material_ui.dart';

import '../access/access_controller.dart';
import '../api/generated/sgt_api.g.dart';
import '../auth/auth_service.dart';
import '../features/floor/floor_view_models.dart';
import '../features/loyalty/loyalty_repository.dart';
import '../features/players/players_repository.dart';
import '../features/reports/daily_summary_view_model.dart';

/// Uygulama servisleri (bağımlılıklar). Ekranlar `AppServices.of(context)` ile alır; testte sahteleri verilir.
class AppServices {
  AppServices({
    required this.auth,
    required this.api,
    required this.terminalId,
    required this.newId,
  }) : access = AccessController(api);

  final AuthService auth;
  final SgtApiClient api;
  final AccessController access;
  final String terminalId;

  /// İstemci kaynaklı idempotency anahtarı (UUID v4).
  final String Function() newId;

  PlayersRepository get players => PlayersRepository(api);

  LoyaltyRepository get loyalty => LoyaltyRepository(api);

  ReportsRepository get reports => ReportsRepository(api);

  FloorRepository get floor => FloorRepository(api);

  static AppServices of(BuildContext context) =>
      context.dependOnInheritedWidgetOfExactType<ServicesScope>()!.services;
}

class ServicesScope extends InheritedWidget {
  const ServicesScope({super.key, required this.services, required super.child});

  final AppServices services;

  @override
  bool updateShouldNotify(ServicesScope oldWidget) => services != oldWidget.services;
}
