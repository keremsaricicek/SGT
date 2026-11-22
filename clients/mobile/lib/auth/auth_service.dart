import 'package:flutter/foundation.dart';

import 'jwt_claims.dart';
import 'oidc_client.dart';
import 'token_store.dart';

/// Oturumdaki kullanıcı. Kişisel veri yok: yalnızca kimlik, tenant ve kullanıcı adı.
class UserSession {
  const UserSession({required this.userId, required this.tenantId, required this.username});

  final String userId;
  final String tenantId;
  final String username;
}

class AuthException implements Exception {
  const AuthException(this.reason);

  final String reason;

  @override
  String toString() => 'AuthException($reason)';
}

/// Oturum durumu (Spec 014 KK1, KK2). Erişim token'ı bellekte, yenileme token'ı güvenli depoda.
class AuthService extends ChangeNotifier {
  AuthService({required this._oidc, required this._store, DateTime Function()? clock}) : _clock = clock ?? DateTime.now;

  static const refreshMargin = Duration(seconds: 30);

  final OidcClient _oidc;
  final RefreshTokenStore _store;
  final DateTime Function() _clock;

  UserSession? _session;
  String? _accessToken;
  DateTime? _expiresAt;
  String? _refreshToken;
  String? _idToken;
  Future<String?>? _refreshing;

  UserSession? get session => _session;

  bool get isSignedIn => _session != null;

  Future<void> signIn() async => _accept(await _oidc.authorize());

  /// Uygulama açılışında saklı yenileme token'ı varsa oturumu sessizce geri getirir.
  Future<void> restore() async {
    final stored = await _store.read();
    if (stored == null) return;
    _refreshToken = stored.refreshToken;
    _idToken = stored.idToken;
    try {
      await _accept(await _oidc.refresh(stored.refreshToken));
    } on Object {
      await _clear();
    }
  }

  /// Geçerli erişim token'ı; süresi dolmak üzereyse önce yeniler. Yenilenemezse oturum kapanır ve null döner.
  Future<String?> accessToken() async {
    if (_session == null) return null;
    final exp = _expiresAt;
    if (exp != null && _clock().toUtc().isAfter(exp.subtract(refreshMargin))) {
      return _refreshing ??= _refresh().whenComplete(() => _refreshing = null);
    }
    return _accessToken;
  }

  Future<void> signOut() async {
    final id = _idToken;
    final rt = _refreshToken;
    await _clear();
    if (rt != null) {
      try {
        await _oidc.revoke(rt);
      } on Object {
        // İptal edilemese de token yerelden silindi; SSO oturum süresiyle sona erer.
      }
    }
    if (id != null) {
      try {
        await _oidc.endSession(id);
      } on Object {
        // Keycloak'a ulaşılamasa da yerel oturum kapanmıştır.
      }
    }
  }

  /// API 401 döndüğünde çağrılır.
  Future<void> handleUnauthorized() => _clear();

  Future<String?> _refresh() async {
    final rt = _refreshToken;
    if (rt == null) {
      await _clear();
      return null;
    }
    try {
      await _accept(await _oidc.refresh(rt));
      return _accessToken;
    } on Object {
      await _clear();
      return null;
    }
  }

  Future<void> _accept(OidcTokens tokens) async {
    final JwtClaims claims;
    try {
      claims = JwtClaims.parse(tokens.accessToken);
    } on FormatException {
      throw const AuthException('invalid_token');
    }
    final sub = claims.subject;
    final tenant = claims.tenantId;
    if (sub == null || sub.isEmpty || tenant == null || tenant.isEmpty) {
      throw const AuthException('missing_claims');
    }
    _accessToken = tokens.accessToken;
    _expiresAt = claims.expiresAt;
    _refreshToken = tokens.refreshToken ?? _refreshToken;
    _idToken = tokens.idToken ?? _idToken;
    _session = UserSession(userId: sub, tenantId: tenant, username: claims.username ?? '');
    final rt = _refreshToken;
    if (rt != null) await _store.write(StoredTokens(rt, _idToken));
    notifyListeners();
  }

  Future<void> _clear() async {
    final had = _session != null;
    _session = null;
    _accessToken = null;
    _expiresAt = null;
    _refreshToken = null;
    _idToken = null;
    await _store.clear();
    if (had) notifyListeners();
  }
}
