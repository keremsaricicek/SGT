import 'dart:convert';

/// Erişim token'ının istemcinin ihtiyaç duyduğu alanları. İmza API'de doğrulanır (ADR-0003);
/// burada yalnızca oturum bilgisi ve süre okunur.
class JwtClaims {
  const JwtClaims({this.subject, this.tenantId, this.username, this.expiresAt});

  factory JwtClaims.parse(String token) {
    final parts = token.split('.');
    if (parts.length != 3) throw const FormatException('JWT değil');
    final payload = jsonDecode(utf8.decode(base64Url.decode(base64Url.normalize(parts[1]))));
    if (payload is! Map) throw const FormatException('JWT gövdesi nesne değil');
    final exp = payload['exp'];
    return JwtClaims(
      subject: payload['sub'] as String?,
      tenantId: payload['tenant_id'] as String?,
      username: payload['preferred_username'] as String?,
      expiresAt: exp is int ? DateTime.fromMillisecondsSinceEpoch(exp * 1000, isUtc: true) : null,
    );
  }

  final String? subject;
  final String? tenantId;
  final String? username;
  final DateTime? expiresAt;
}
