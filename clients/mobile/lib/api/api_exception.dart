/// API çağrısının başarısızlık türü; ekran mesajı türe göre seçilir (Spec 014 KK5).
enum ApiFailure { network, unauthorized, forbidden, notFound, conflict, validation, server, invalidResponse }

/// API hatası. [title] ve [code] sunucunun problem+json gövdesinden gelir; kişisel veri içermez.
class ApiException implements Exception {
  const ApiException(this.kind, {this.status, this.title, this.code});

  factory ApiException.fromStatus(int status, {String? title, String? code}) => ApiException(
        switch (status) {
          401 => ApiFailure.unauthorized,
          403 => ApiFailure.forbidden,
          404 => ApiFailure.notFound,
          409 => ApiFailure.conflict,
          400 || 422 => ApiFailure.validation,
          _ => ApiFailure.server,
        },
        status: status,
        title: title,
        code: code,
      );

  final ApiFailure kind;
  final int? status;
  final String? title;
  final String? code;

  @override
  String toString() => 'ApiException(${kind.name}, status: $status, code: $code)';
}
