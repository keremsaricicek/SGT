import 'package:flutter_test/flutter_test.dart';
import 'package:material_ui/material_ui.dart';
import 'package:sgt_mobile/app/router.dart';
import 'package:sgt_mobile/features/login/login_screen.dart';
import 'package:sgt_mobile/theme/sgt_theme.dart';
import 'package:sgt_mobile/theme/sgt_tokens.dart';

import '../support/test_app.dart';

void main() {
  testWidgets('oturum yoksa giriş ekranı açılır, şifre alanı yoktur (KK1)', (tester) async {
    await pumpApp(tester, await testServices(sampleTransport(), signedIn: false), location: '/floor/tables');
    expect(find.byType(LoginScreen), findsOneWidget);
    expect(find.byType(TextField), findsNothing);
  });

  testWidgets('menü yalnızca yetkili ve mobilde karşılığı olan ekranları listeler (KK3)', (tester) async {
    final services = await testServices(sampleTransport(menu: const [
      {'screen': 'player.search', 'actions': ['view']},
      {'screen': 'report.daily', 'actions': ['view']},
      {'screen': 'cage.shift', 'actions': ['view', 'open']},
      {'screen': 'loyalty.host_portfolio', 'actions': ['view']},
    ]));
    await pumpApp(tester, services);
    expect(find.text('Oyuncu arama'), findsOneWidget);
    expect(find.text('Günlük özet'), findsOneWidget);
    expect(find.text('Masa haritası'), findsNothing);
    expect(find.text('Slot salonu'), findsNothing);
    expect(find.textContaining('cage.'), findsNothing);
    expect(find.textContaining('host_portfolio'), findsNothing);
  });

  testWidgets('menüde olmayan ekran doğrudan açılsa da yetkiniz yok gösterir, API çağrılmaz', (tester) async {
    final transport = sampleTransport(menu: const [
      {'screen': 'player.search', 'actions': ['view']},
    ]);
    await pumpApp(tester, await testServices(transport), location: '/floor/slots');
    expect(find.byType(NoAccessScreen), findsOneWidget);
    expect(transport.count('GET', '/api/v1/floor/slots'), 0);
  });

  testWidgets('comp talep düğmesi yalnızca loyalty.comp:request varsa görünür', (tester) async {
    await pumpApp(
      tester,
      await testServices(sampleTransport(menu: const [
        {'screen': 'player.locator', 'actions': ['view']},
        {'screen': 'loyalty.comp', 'actions': ['view']},
      ])),
    );
    await push(tester, '/players/0b7c2d1e-0000-4000-8000-000000000101');
    expect(find.text('Comp talep et'), findsNothing);
  });

  testWidgets('İngilizce arayüz', (tester) async {
    await pumpApp(tester, await testServices(sampleTransport()), locale: const Locale('en'));
    expect(find.text('Player search'), findsOneWidget);
    expect(find.text('Daily summary'), findsOneWidget);
  });

  test('tema: köşe yarıçapı ≤ 3 px, hap düğme yok (KK16)', () {
    expect(SgtTokens.radius, lessThanOrEqualTo(3));
    for (final theme in [SgtTheme.light(), SgtTheme.dark()]) {
      for (final style in [
        theme.filledButtonTheme.style,
        theme.outlinedButtonTheme.style,
        theme.textButtonTheme.style,
        theme.iconButtonTheme.style,
      ]) {
        final shape = style!.shape!.resolve({})! as RoundedRectangleBorder;
        final r = (shape.borderRadius as BorderRadius).topLeft.x;
        expect(r, lessThanOrEqualTo(3));
      }
      final input = theme.inputDecorationTheme.border! as OutlineInputBorder;
      expect(input.borderRadius.topLeft.x, lessThanOrEqualTo(3));
      expect(theme.splashFactory, NoSplash.splashFactory);
    }
  });
}
