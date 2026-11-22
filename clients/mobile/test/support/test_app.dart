import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:material_ui/material_ui.dart';
import 'package:sgt_mobile/api/generated/sgt_api.g.dart';
import 'package:sgt_mobile/app/services.dart';
import 'package:sgt_mobile/app/sgt_app.dart';
import 'package:sgt_mobile/auth/auth_service.dart';
import 'package:sgt_mobile/auth/oidc_client.dart';
import 'package:sgt_mobile/auth/token_store.dart';

import 'fake_transport.dart';
import 'sample_data.dart' as s;

class _Oidc implements OidcClient {
  @override
  Future<OidcTokens> authorize() async {
    String part(Object o) => base64Url.encode(utf8.encode(jsonEncode(o))).replaceAll('=', '');
    final token = '${part({'alg': 'none'})}.${part({
      'sub': '11111111-1111-4111-8111-111111111111',
      'tenant_id': '22222222-2222-4222-8222-222222222222',
      'preferred_username': 'host.demo',
      'exp': 4102444800, // 2100
    })}.x';
    return OidcTokens(accessToken: token, refreshToken: 'rt', idToken: 'id');
  }

  @override
  Future<OidcTokens> refresh(String refreshToken) => authorize();

  @override
  Future<void> endSession(String idToken) async {}

  @override
  Future<void> revoke(String refreshToken) async {}
}

class _Store implements RefreshTokenStore {
  @override
  Future<void> clear() async {}

  @override
  Future<StoredTokens?> read() async => null;

  @override
  Future<void> write(StoredTokens tokens) async {}
}

/// Sentetik verili sahte API.
FakeTransport sampleTransport({List<Map<String, Object>> menu = s.menuAll}) => FakeTransport()
  ..reply('GET', '/api/v1/identity/me/menu', menu)
  ..reply('GET', '/api/v1/players', s.players)
  ..reply('GET', '/api/v1/floor/players/${s.p1}/location', s.location)
  ..reply('GET', '/api/v1/loyalty/players/${s.p1}', s.loyalty)
  ..reply('POST', '/api/v1/loyalty/comps', {'id': 'r'})
  ..reply('GET', '/api/v1/reports/daily', s.daily)
  ..reply('GET', '/api/v1/floor/tables', s.tables())
  ..reply('GET', '/api/v1/floor/ratings/running', s.runningRatings())
  ..reply('GET', '/api/v1/floor/slots', s.slots());

Future<AppServices> testServices(FakeTransport transport, {bool signedIn = true}) async {
  final auth = AuthService(oidc: _Oidc(), store: _Store());
  if (signedIn) await auth.signIn();
  var n = 0;
  return AppServices(
    auth: auth,
    api: SgtApiClient(transport),
    terminalId: 'M-3fa2c91b',
    newId: () => 'req-${++n}',
  );
}

/// Telefon boyutu (390×844 mantıksal, 2x).
void phoneView(WidgetTester tester) {
  tester.view.physicalSize = const Size(780, 1688);
  tester.view.devicePixelRatio = 2;
  addTearDown(tester.view.reset);
}

Future<void> pumpApp(
  WidgetTester tester,
  AppServices services, {
  String location = '/',
  ThemeMode theme = ThemeMode.light,
  Locale locale = const Locale('tr'),
}) async {
  phoneView(tester);
  await tester.pumpWidget(SgtApp(services: services, initialLocation: location, themeMode: theme, locale: locale));
  await settle(tester);
}

/// pumpAndSettle yerine: imleç yanıp sönmesi sonsuz animasyondur.
Future<void> settle(WidgetTester tester) async {
  for (var i = 0; i < 6; i++) {
    await tester.pump(const Duration(milliseconds: 50));
  }
}

Future<void> push(WidgetTester tester, String location, {Object? extra}) async {
  final context = tester.element(find.byType(Navigator).first);
  GoRouter.of(context).push(location, extra: extra);
  await settle(tester);
}
