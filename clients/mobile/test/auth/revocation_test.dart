import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:sgt_mobile/auth/oidc_client.dart';

void main() {
  test('yenileme token\'ı Keycloak revocation ucuna form olarak gönderilir', () async {
    late http.Request sent;
    final client = AppAuthOidcClient(
      const OidcSettings(issuer: 'https://giris.ornek/realms/sgt', clientId: 'sgt-mobile', redirectUri: 'x:/cb'),
      http: MockClient((r) async {
        sent = r;
        return http.Response('', 200);
      }),
    );
    await client.revoke('rt-9');
    expect(sent.method, 'POST');
    expect(sent.url.toString(), 'https://giris.ornek/realms/sgt/protocol/openid-connect/revoke');
    expect(sent.bodyFields, {'token': 'rt-9', 'token_type_hint': 'refresh_token', 'client_id': 'sgt-mobile'});
  });

  test('revocation hata dönerse istisna fırlatılır (çağıran yine de yerel oturumu kapatır)', () async {
    final client = AppAuthOidcClient(
      const OidcSettings(issuer: 'https://giris.ornek/realms/sgt/', clientId: 'sgt-mobile', redirectUri: 'x:/cb'),
      http: MockClient((_) async => http.Response('', 503)),
    );
    await expectLater(client.revoke('rt'), throwsA(isA<StateError>()));
  });
}
