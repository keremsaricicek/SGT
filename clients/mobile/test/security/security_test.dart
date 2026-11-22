import 'dart:convert';
import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/io_client.dart';
import 'package:sgt_mobile/config/app_config.dart';
import 'package:sgt_mobile/security/certificate_pinning.dart';
import 'package:sgt_mobile/security/terminal_id.dart';

class MemoryKv implements TerminalIdStore {
  String? value;

  @override
  Future<String?> read() async => value;

  @override
  Future<void> write(String id) async => value = id;
}

void main() {
  test('terminal kimliği M- + 8 hex, cihaz adı değil ve kalıcı (KK4)', () async {
    final kv = MemoryKv();
    final first = await TerminalId.load(kv);
    expect(first, matches(RegExp(r'^M-[0-9a-f]{8}$')));
    expect(await TerminalId.load(kv), first);
  });

  test('sabitlenmiş sertifika verilirse yalnızca ona güvenen istemci kurulur (KK17)', () {
    final pem = File('test/fixtures/test-ca.pem').readAsBytesSync();
    expect(PinnedHttpClientFactory.create(pem), isA<IOClient>());
  });

  test('geçersiz sertifika verisi reddedilir', () {
    expect(() => PinnedHttpClientFactory.create(utf8.encode('sertifika değil')), throwsA(isA<TlsException>()));
  });

  group('AppConfig', () {
    test('varsayılan istemci sgt-mobile ve dönüş adresi', () {
      final c = AppConfig.from(apiBase: 'https://api.ornek/', issuer: 'https://giris.ornek/realms/sgt');
      expect(c.clientId, 'sgt-mobile');
      expect(c.redirectUri, 'com.sentezgaming.sgt:/oauthredirect');
    });

    test('üretimde http adres kabul edilmez', () {
      expect(
        () => AppConfig.from(apiBase: 'http://api.ornek/', issuer: 'https://giris.ornek/realms/sgt', production: true),
        throwsArgumentError,
      );
      expect(
        () => AppConfig.from(apiBase: 'https://api.ornek/', issuer: 'http://giris.ornek/realms/sgt', production: true),
        throwsArgumentError,
      );
    });

    test('üretim varsayılanı derleme kipidir (release → üretim, yalnızca HTTPS)', () {
      final c = AppConfig.from(apiBase: 'https://api.ornek/', issuer: 'https://giris.ornek/realms/sgt');
      expect(c.production, kReleaseMode);
      expect(AppConfig.productionFromEnvironment, kReleaseMode); // --dart-define=SGT_PRODUCTION verilmediğinde
    });

    test('adres verilmemişse açık hata', () {
      expect(() => AppConfig.from(apiBase: '', issuer: ''), throwsArgumentError);
    });
  });
}
