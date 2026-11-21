import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:sgt_mobile/auth/auth_service.dart';
import 'package:sgt_mobile/auth/jwt_claims.dart';
import 'package:sgt_mobile/auth/oidc_client.dart';
import 'package:sgt_mobile/auth/token_store.dart';

String jwt(Map<String, Object?> claims) {
  String part(Object o) => base64Url.encode(utf8.encode(jsonEncode(o))).replaceAll('=', '');
  return '${part({'alg': 'RS256'})}.${part(claims)}.imza';
}

final t0 = DateTime.utc(2026, 11, 21, 10);

String access({int expInSeconds = 300, bool tenant = true, bool sub = true}) => jwt({
      if (sub) 'sub': '11111111-1111-4111-8111-111111111111',
      if (tenant) 'tenant_id': '22222222-2222-4222-8222-222222222222',
      'preferred_username': 'host1',
      'exp': t0.add(Duration(seconds: expInSeconds)).millisecondsSinceEpoch ~/ 1000,
    });

class FakeOidc implements OidcClient {
  OidcTokens? next;
  Object? refreshError;
  int refreshes = 0;
  String? endedWith;

  @override
  Future<OidcTokens> authorize() async => next!;

  @override
  Future<OidcTokens> refresh(String refreshToken) async {
    refreshes++;
    if (refreshError != null) throw refreshError!;
    return next!;
  }

  @override
  Future<void> endSession(String idToken) async => endedWith = idToken;
}

class MemoryStore implements RefreshTokenStore {
  String? value;
  String? idToken;

  @override
  Future<StoredTokens?> read() async => value == null ? null : StoredTokens(value!, idToken);

  @override
  Future<void> write(StoredTokens tokens) async {
    value = tokens.refreshToken;
    idToken = tokens.idToken;
  }

  @override
  Future<void> clear() async {
    value = null;
    idToken = null;
  }
}

void main() {
  late FakeOidc oidc;
  late MemoryStore store;
  late DateTime now;
  late AuthService auth;

  setUp(() {
    oidc = FakeOidc();
    store = MemoryStore();
    now = t0;
    auth = AuthService(oidc: oidc, store: store, clock: () => now);
  });

  test('JWT sub, tenant_id ve kullanıcı adını okur', () {
    final c = JwtClaims.parse(access());
    expect(c.subject, '11111111-1111-4111-8111-111111111111');
    expect(c.tenantId, '22222222-2222-4222-8222-222222222222');
    expect(c.username, 'host1');
    expect(c.expiresAt, t0.add(const Duration(minutes: 5)));
  });

  test('giriş: oturum açılır, yenileme token\'ı yalnızca güvenli depoya yazılır (KK2)', () async {
    oidc.next = OidcTokens(accessToken: access(), refreshToken: 'rt-1', idToken: 'id-1');
    await auth.signIn();

    expect(auth.session?.username, 'host1');
    expect(auth.session?.tenantId, '22222222-2222-4222-8222-222222222222');
    expect(store.value, 'rt-1');
    expect(await auth.accessToken(), isNotNull);
  });

  test('tenant_id yoksa oturum açılmaz', () async {
    oidc.next = OidcTokens(accessToken: access(tenant: false), refreshToken: 'rt', idToken: 'id');
    await expectLater(auth.signIn(), throwsA(isA<AuthException>()));
    expect(auth.session, isNull);
    expect(store.value, isNull);
  });

  test('sub yoksa oturum açılmaz', () async {
    oidc.next = OidcTokens(accessToken: access(sub: false), refreshToken: 'rt', idToken: 'id');
    await expectLater(auth.signIn(), throwsA(isA<AuthException>()));
  });

  test('süresi dolmak üzere olan token istekten önce yenilenir', () async {
    oidc.next = OidcTokens(accessToken: access(expInSeconds: 60), refreshToken: 'rt-1', idToken: 'id-1');
    await auth.signIn();
    now = t0.add(const Duration(seconds: 40)); // 20 sn kaldı (< 30 sn)
    oidc.next = OidcTokens(accessToken: access(expInSeconds: 400), refreshToken: 'rt-2', idToken: 'id-2');

    await auth.accessToken();
    expect(oidc.refreshes, 1);
    expect(store.value, 'rt-2');
  });

  test('yenileme başarısızsa oturum kapanır ve depo temizlenir', () async {
    oidc.next = OidcTokens(accessToken: access(expInSeconds: 10), refreshToken: 'rt-1', idToken: 'id-1');
    await auth.signIn();
    oidc.refreshError = Exception('invalid_grant');

    expect(await auth.accessToken(), isNull);
    expect(auth.session, isNull);
    expect(store.value, isNull);
  });

  test('uygulama açılışında saklı yenileme token\'ı ile oturum geri gelir', () async {
    store.value = 'rt-saved';
    oidc.next = OidcTokens(accessToken: access(), refreshToken: 'rt-new', idToken: 'id');
    await auth.restore();
    expect(auth.session, isNotNull);
    expect(store.value, 'rt-new');
  });

  test('saklı token yoksa restore oturumu kapalı bırakır', () async {
    await auth.restore();
    expect(auth.session, isNull);
    expect(oidc.refreshes, 0);
  });

  test('çıkış: depo silinir, Keycloak oturumu kapatılır', () async {
    oidc.next = OidcTokens(accessToken: access(), refreshToken: 'rt-1', idToken: 'id-1');
    await auth.signIn();
    await auth.signOut();
    expect(auth.session, isNull);
    expect(store.value, isNull);
    expect(oidc.endedWith, 'id-1');
  });

  test('401 bildirimi oturumu kapatır ve dinleyicileri uyarır', () async {
    oidc.next = OidcTokens(accessToken: access(), refreshToken: 'rt-1', idToken: 'id-1');
    await auth.signIn();
    var notified = 0;
    auth.addListener(() => notified++);
    await auth.handleUnauthorized();
    expect(auth.session, isNull);
    expect(notified, greaterThan(0));
  });
}
