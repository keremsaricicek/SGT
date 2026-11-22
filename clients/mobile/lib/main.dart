import 'package:flutter/services.dart';
import 'package:http/http.dart' as http;
import 'package:material_ui/material_ui.dart';
import 'package:timezone/data/latest.dart' as tzdata;
import 'package:uuid/uuid.dart';

import 'api/api_transport.dart';
import 'api/generated/sgt_api.g.dart';
import 'app/services.dart';
import 'app/sgt_app.dart';
import 'auth/auth_service.dart';
import 'auth/oidc_client.dart';
import 'auth/token_store.dart';
import 'config/app_config.dart';
import 'security/certificate_pinning.dart';
import 'security/terminal_id.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  tzdata.initializeTimeZones();

  final config = AppConfig.fromEnvironment();
  final terminalId = await TerminalId.load(const SecureTerminalIdStore());
  final pinned = config.pinnedCertAsset;
  final client = pinned == null
      ? http.Client()
      : PinnedHttpClientFactory.create((await rootBundle.load(pinned)).buffer.asUint8List());
  final auth = AuthService(
    oidc: AppAuthOidcClient(
      OidcSettings(issuer: config.issuer, clientId: config.clientId, redirectUri: config.redirectUri),
      http: client,
    ),
    store: SecureRefreshTokenStore(),
  );

  const uuid = Uuid();
  final transport = HttpApiTransport(
    client: client,
    baseUri: config.apiBase,
    accessToken: auth.accessToken,
    terminalId: terminalId,
    newCorrelationId: uuid.v4,
    onUnauthorized: auth.handleUnauthorized,
    requireHttps: config.production,
  );

  await auth.restore();
  runApp(SgtApp(
    services: AppServices(auth: auth, api: SgtApiClient(transport), terminalId: terminalId, newId: uuid.v4),
  ));
}
