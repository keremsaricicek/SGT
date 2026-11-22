// Her ekranın koyu ve açık golden görüntüsü (KK18), sentetik veriyle.
// Güncelleme: flutter test --update-goldens test/goldens  (Linux'ta üretilir; platformlar arası piksel farkı olabilir)
import 'package:flutter_test/flutter_test.dart';
import 'package:material_ui/material_ui.dart';
import 'package:sgt_mobile/api/generated/sgt_api.g.dart';
import 'package:sgt_mobile/app/sgt_app.dart';

import '../support/sample_data.dart' as s;
import '../support/test_app.dart';

final summary = PlayerSummary.fromJson(s.players.first);

void main() {
  const themes = {'light': ThemeMode.light, 'dark': ThemeMode.dark};

  for (final MapEntry(key: name, value: mode) in themes.entries) {
    testWidgets('giris-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport(), signedIn: false), theme: mode);
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/giris-$name.png'));
    });

    testWidgets('ana-menu-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/ana-menu-$name.png'));
    });

    testWidgets('oyuncu-arama-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await push(tester, '/players');
      await tester.enterText(find.byType(TextField), 'deniz');
      await tester.testTextInput.receiveAction(TextInputAction.search);
      await settle(tester);
      FocusManager.instance.primaryFocus?.unfocus();
      await settle(tester);
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/oyuncu-arama-$name.png'));
    });

    testWidgets('oyuncu-konum-puan-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await push(tester, '/players/${s.p1}', extra: summary);
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/oyuncu-konum-puan-$name.png'));
    });

    testWidgets('comp-talebi-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await push(tester, '/players/${s.p1}/comp', extra: summary);
      final fields = find.byType(TextField);
      await tester.enterText(fields.at(0), '1500');
      await tester.enterText(fields.at(1), 'Akşam yemeği, 2 kişi');
      await tester.enterText(fields.at(2), 'comp_yemek');
      FocusManager.instance.primaryFocus?.unfocus();
      await settle(tester);
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/comp-talebi-$name.png'));
    });

    testWidgets('gunluk-ozet-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await push(tester, '/reports/daily');
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/gunluk-ozet-$name.png'));
    });

    testWidgets('masa-haritasi-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await push(tester, '/floor/tables');
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/masa-haritasi-$name.png'));
    });

    testWidgets('slot-salonu-$name', (tester) async {
      await pumpApp(tester, await testServices(sampleTransport()), theme: mode);
      await push(tester, '/floor/slots');
      await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/slot-salonu-$name.png'));
    });
  }

  testWidgets('ana-menu-en-dark (İngilizce)', (tester) async {
    await pumpApp(tester, await testServices(sampleTransport()), theme: ThemeMode.dark, locale: const Locale('en'));
    await expectLater(find.byType(SgtApp), matchesGoldenFile('goldens/ana-menu-en-dark.png'));
  });
}
