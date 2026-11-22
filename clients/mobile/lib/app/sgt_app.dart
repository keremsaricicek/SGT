import 'package:flutter_localizations/flutter_localizations.dart' show GlobalWidgetsLocalizations;
import 'package:cupertino_ui/cupertino_ui.dart' show GlobalCupertinoLocalizations;
import 'package:go_router/go_router.dart';
import 'package:material_ui/material_ui.dart';

import '../l10n/gen/app_localizations.dart';
import '../theme/sgt_theme.dart';
import 'router.dart';
import 'services.dart';

/// Kök widget: tema (koyu/açık cihaza göre), TR/EN, rotalar. Oturum açılınca menü yüklenir.
class SgtApp extends StatefulWidget {
  const SgtApp({super.key, required this.services, this.initialLocation = '/', this.themeMode = ThemeMode.system, this.locale});

  final AppServices services;
  final String initialLocation;
  final ThemeMode themeMode;
  final Locale? locale;

  @override
  State<SgtApp> createState() => _SgtAppState();
}

class _SgtAppState extends State<SgtApp> {
  late final GoRouter _router = buildRouter(widget.services, initialLocation: widget.initialLocation);
  bool _wasSignedIn = false;

  @override
  void initState() {
    super.initState();
    widget.services.auth.addListener(_onAuth);
    _onAuth();
  }

  void _onAuth() {
    final signedIn = widget.services.auth.isSignedIn;
    if (signedIn && !_wasSignedIn) widget.services.access.load();
    if (!signedIn && _wasSignedIn) widget.services.access.clear();
    _wasSignedIn = signedIn;
  }

  @override
  void dispose() {
    widget.services.auth.removeListener(_onAuth);
    _router.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => ServicesScope(
        services: widget.services,
        child: MaterialApp.router(
          onGenerateTitle: (c) => AppLocalizations.of(c).appTitle,
          debugShowCheckedModeBanner: false,
          theme: SgtTheme.light(),
          darkTheme: SgtTheme.dark(),
          themeMode: widget.themeMode,
          locale: widget.locale,
          supportedLocales: AppLocalizations.supportedLocales,
          localizationsDelegates: const [
            AppLocalizations.delegate,
            GlobalMaterialLocalizations.delegate,
            GlobalCupertinoLocalizations.delegate,
            GlobalWidgetsLocalizations.delegate,
          ],
          routerConfig: _router,
        ),
      );
}
