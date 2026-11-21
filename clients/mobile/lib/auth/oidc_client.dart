import 'package:flutter_appauth/flutter_appauth.dart';

/// Token üçlüsü (Keycloak yanıtı).
class OidcTokens {
  const OidcTokens({required this.accessToken, this.refreshToken, this.idToken});

  final String accessToken;
  final String? refreshToken;
  final String? idToken;
}

/// OIDC sağlayıcı bağlantısı. Uygulama kodu flutter_appauth'a doğrudan bağlanmaz (testte sahtesi verilir).
abstract interface class OidcClient {
  /// Sistem tarayıcısında Authorization Code + PKCE (S256) girişi.
  Future<OidcTokens> authorize();

  Future<OidcTokens> refresh(String refreshToken);

  Future<void> endSession(String idToken);
}

/// Keycloak OIDC ayarları (`--dart-define` ile verilir; kodda adres/secret yok).
class OidcSettings {
  const OidcSettings({required this.issuer, required this.clientId, required this.redirectUri});

  final String issuer;
  final String clientId;
  final String redirectUri;

  static const scopes = ['openid', 'profile', 'offline_access'];
}

/// flutter_appauth adaptörü. PKCE'yi AppAuth kütüphanesi üretir; şifre uygulamaya hiç girmez (KK1).
class AppAuthOidcClient implements OidcClient {
  AppAuthOidcClient(this._settings, [FlutterAppAuth? appAuth]) : _appAuth = appAuth ?? const FlutterAppAuth();

  final OidcSettings _settings;
  final FlutterAppAuth _appAuth;

  @override
  Future<OidcTokens> authorize() async {
    final r = await _appAuth.authorizeAndExchangeCode(
      AuthorizationTokenRequest(
        _settings.clientId,
        _settings.redirectUri,
        issuer: _settings.issuer,
        scopes: OidcSettings.scopes,
        allowInsecureConnections: false,
      ),
    );
    return _tokens(r.accessToken, r.refreshToken, r.idToken);
  }

  @override
  Future<OidcTokens> refresh(String refreshToken) async {
    final r = await _appAuth.token(
      TokenRequest(
        _settings.clientId,
        _settings.redirectUri,
        issuer: _settings.issuer,
        refreshToken: refreshToken,
        scopes: OidcSettings.scopes,
      ),
    );
    return _tokens(r.accessToken, r.refreshToken, r.idToken);
  }

  @override
  Future<void> endSession(String idToken) => _appAuth.endSession(
        EndSessionRequest(
          idTokenHint: idToken,
          issuer: _settings.issuer,
          postLogoutRedirectUrl: _settings.redirectUri,
        ),
      );

  static OidcTokens _tokens(String? access, String? refresh, String? id) {
    if (access == null || access.isEmpty) throw StateError('Erişim token\'ı alınamadı');
    return OidcTokens(accessToken: access, refreshToken: refresh, idToken: id);
  }
}
