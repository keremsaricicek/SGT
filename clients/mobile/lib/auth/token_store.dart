import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Kalıcı saklanan tek şey: yenileme token'ı (+ çıkış için id token). Erişim token'ı yalnızca bellektedir.
class StoredTokens {
  const StoredTokens(this.refreshToken, this.idToken);

  final String refreshToken;
  final String? idToken;
}

abstract interface class RefreshTokenStore {
  Future<StoredTokens?> read();

  Future<void> write(StoredTokens tokens);

  Future<void> clear();
}

/// Android Keystore / iOS Keychain (flutter_secure_storage). Spec 014 KK2, MASVS-STORAGE.
class SecureRefreshTokenStore implements RefreshTokenStore {
  SecureRefreshTokenStore([FlutterSecureStorage? storage])
      : _storage = storage ??
            const FlutterSecureStorage(
              iOptions: IOSOptions(accessibility: KeychainAccessibility.first_unlock_this_device),
            );

  static const _refreshKey = 'sgt.refresh_token';
  static const _idKey = 'sgt.id_token';

  final FlutterSecureStorage _storage;

  @override
  Future<StoredTokens?> read() async {
    final refresh = await _storage.read(key: _refreshKey);
    if (refresh == null || refresh.isEmpty) return null;
    return StoredTokens(refresh, await _storage.read(key: _idKey));
  }

  @override
  Future<void> write(StoredTokens tokens) async {
    await _storage.write(key: _refreshKey, value: tokens.refreshToken);
    if (tokens.idToken != null) await _storage.write(key: _idKey, value: tokens.idToken);
  }

  @override
  Future<void> clear() async {
    await _storage.delete(key: _refreshKey);
    await _storage.delete(key: _idKey);
  }
}
