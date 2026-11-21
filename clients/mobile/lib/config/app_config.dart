/// Ortam ayarları. Değerler derlemede `--dart-define` ile verilir; kodda adres veya secret yoktur (KK17).
///
/// ```
/// flutter run --dart-define=SGT_API_BASE=https://sgt-api.ornek-casino.local/ \
///   --dart-define=SGT_AUTH_ISSUER=https://giris.ornek-casino.local/realms/sgt --dart-define=SGT_PRODUCTION=true
/// ```
class AppConfig {
  const AppConfig._({
    required this.apiBase,
    required this.issuer,
    required this.clientId,
    required this.redirectUri,
    required this.production,
    this.pinnedCertAsset,
  });

  factory AppConfig.from({
    required String apiBase,
    required String issuer,
    String clientId = defaultClientId,
    bool production = false,
    String? pinnedCertAsset,
  }) {
    final api = Uri.tryParse(apiBase);
    final iss = Uri.tryParse(issuer);
    if (apiBase.isEmpty || api == null || !api.hasScheme) {
      throw ArgumentError.value(apiBase, 'SGT_API_BASE', 'API adresi verilmeli');
    }
    if (issuer.isEmpty || iss == null || !iss.hasScheme) {
      throw ArgumentError.value(issuer, 'SGT_AUTH_ISSUER', 'Keycloak realm adresi verilmeli');
    }
    if (production && (api.scheme != 'https' || iss.scheme != 'https')) {
      throw ArgumentError('Üretimde yalnızca HTTPS adresler kullanılır');
    }
    return AppConfig._(
      apiBase: apiBase.endsWith('/') ? api : Uri.parse('$apiBase/'),
      issuer: issuer,
      clientId: clientId,
      redirectUri: redirect,
      production: production,
      pinnedCertAsset: (pinnedCertAsset?.isEmpty ?? true) ? null : pinnedCertAsset,
    );
  }

  factory AppConfig.fromEnvironment() => AppConfig.from(
        apiBase: const String.fromEnvironment('SGT_API_BASE'),
        issuer: const String.fromEnvironment('SGT_AUTH_ISSUER'),
        clientId: const String.fromEnvironment('SGT_CLIENT_ID', defaultValue: defaultClientId),
        production: const bool.fromEnvironment('SGT_PRODUCTION'),
        pinnedCertAsset: const String.fromEnvironment('SGT_PINNED_CERT'),
      );

  static const defaultClientId = 'sgt-mobile';
  static const redirect = 'com.sentezgaming.sgt:/oauthredirect';

  final Uri apiBase;
  final String issuer;
  final String clientId;
  final String redirectUri;
  final bool production;
  final String? pinnedCertAsset;
}
