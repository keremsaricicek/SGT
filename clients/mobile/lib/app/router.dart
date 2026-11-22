import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:go_router/go_router.dart';
import 'package:material_ui/material_ui.dart';

import '../access/screen_codes.dart';
import '../api/generated/sgt_api.g.dart';
import '../features/floor/slots_screen.dart';
import '../features/floor/tables_screen.dart';
import '../features/home/home_screen.dart';
import '../features/login/login_screen.dart';
import '../features/loyalty/comp_request_screen.dart';
import '../features/players/player_screen.dart';
import '../features/players/player_search_screen.dart';
import '../features/reports/daily_summary_screen.dart';
import '../shared/load_state.dart';
import '../ui/sgt_widgets.dart';
import 'services.dart';

/// Rotalar. Oturum yoksa /login; her ekran menüdeki yetkiye göre korunur (KK3).
GoRouter buildRouter(AppServices services, {String initialLocation = '/'}) => GoRouter(
      initialLocation: initialLocation,
      refreshListenable: services.auth,
      redirect: (context, state) {
        final signedIn = services.auth.isSignedIn;
        final atLogin = state.matchedLocation == '/login';
        if (!signedIn) return atLogin ? null : '/login';
        return atLogin ? '/' : null;
      },
      routes: [
        GoRoute(path: '/login', builder: (_, _) => const LoginScreen()),
        GoRoute(path: '/', builder: (_, _) => const HomeScreen()),
        _search('/players', ScreenCodes.playerSearch, SearchPurpose.player),
        _search('/locator', ScreenCodes.playerLocator, SearchPurpose.locator),
        _search('/loyalty', ScreenCodes.loyaltyPoints, SearchPurpose.loyalty),
        _search('/comp', ScreenCodes.loyaltyComp, SearchPurpose.comp),
        GoRoute(
          path: '/players/:id',
          builder: (_, s) => ScreenGuard(
            anyOf: const [ScreenCodes.playerLocator, ScreenCodes.loyaltyPoints],
            child: PlayerScreen(playerId: s.pathParameters['id']!, summary: s.extra as PlayerSummary?),
          ),
        ),
        GoRoute(
          path: '/players/:id/comp',
          builder: (_, s) => ScreenGuard(
            anyOf: const [ScreenCodes.loyaltyComp],
            action: ScreenActions.request,
            child: CompRequestScreen(playerId: s.pathParameters['id']!, summary: s.extra as PlayerSummary?),
          ),
        ),
        GoRoute(
          path: '/reports/daily',
          builder: (_, _) => const ScreenGuard(anyOf: [ScreenCodes.reportDaily], child: DailySummaryScreen()),
        ),
        GoRoute(
          path: '/floor/tables',
          builder: (_, _) => const ScreenGuard(anyOf: [ScreenCodes.liveTables], child: TablesScreen()),
        ),
        GoRoute(
          path: '/floor/slots',
          builder: (_, _) => const ScreenGuard(anyOf: [ScreenCodes.slotFloor], child: SlotsScreen()),
        ),
      ],
    );

GoRoute _search(String path, String screen, SearchPurpose purpose) => GoRoute(
      path: path,
      builder: (_, _) => ScreenGuard(
        // Arama ucu player.search:view ister; konum/puan/comp menüsü de aramayı kullanır (spec 014 eksikler).
        anyOf: [screen],
        child: PlayerSearchScreen(purpose: purpose),
      ),
    );

/// Menüde olmayan ekran doğrudan açılsa da "yetkiniz yok" gösterir.
class ScreenGuard extends StatelessWidget {
  const ScreenGuard({super.key, required this.anyOf, this.action, required this.child});

  final List<String> anyOf;
  final String? action;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final access = AppServices.of(context).access;
    return ListenableBuilder(
      listenable: access,
      builder: (context, _) {
        if (access.state case Idle() || Loading()) {
          return const Scaffold(body: LoadingPanel());
        }
        final a = access.access;
        final ok = anyOf.any((s) => action == null ? a.canView(s) : a.can(s, action!));
        return ok ? child : const NoAccessScreen();
      },
    );
  }
}

class NoAccessScreen extends StatelessWidget {
  const NoAccessScreen({super.key});

  @override
  Widget build(BuildContext context) => SgtPage(
        title: context.l10n.noAccessTitle,
        body: MessagePanel(icon: FluentIcons.lock_closed_24_regular, text: context.l10n.noAccessBody, tone: TagTone.warning),
      );
}
