import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import 'api_exception.dart';

/// Ham HTTP yanıtı: durum kodu ve çözülmüş JSON gövdesi (boşsa null).
class ApiResponse {
  const ApiResponse(this.status, this.body);

  final int status;
  final Object? body;
}

/// Üretilen [SgtApiClient]'ın kullandığı taşıma katmanı. Testte sahtesi verilir.
abstract interface class ApiTransport {
  Future<ApiResponse> send(String method, String path, {Map<String, String?> query, Object? body});
}

/// package:http üzerinde taşıma: bearer token, X-Terminal, X-Correlation-Id (ADR-0003, Spec 014 KK4),
/// problem+json → [ApiException] (KK5). Gövde veya token loglanmaz.
class HttpApiTransport implements ApiTransport {
  HttpApiTransport({
    required this._client,
    required this._baseUri,
    required this._accessToken,
    required this._terminalId,
    required this._newCorrelationId,
    this._onUnauthorized,
    bool requireHttps = false,
    this._timeout = const Duration(seconds: 20),
  }) {
    if (requireHttps && _baseUri.scheme != 'https') {
      throw ArgumentError.value(_baseUri.scheme, 'baseUri', 'Üretimde yalnızca HTTPS');
    }
  }

  static const terminalHeader = 'X-Terminal';
  static const correlationHeader = 'X-Correlation-Id';

  final http.Client _client;
  final Uri _baseUri;
  final Future<String?> Function() _accessToken;
  final String _terminalId;
  final String Function() _newCorrelationId;
  final void Function()? _onUnauthorized;
  final Duration _timeout;

  @override
  Future<ApiResponse> send(String method, String path, {Map<String, String?> query = const {}, Object? body}) async {
    final params = {for (final e in query.entries) if (e.value != null) e.key: e.value!};
    final resolved = _baseUri.resolve(path.startsWith('/') ? path.substring(1) : path);
    final uri = params.isEmpty ? resolved : resolved.replace(queryParameters: params);

    final request = http.Request(method, uri)
      ..headers['Accept'] = 'application/json'
      ..headers[terminalHeader] = _terminalId
      ..headers[correlationHeader] = _newCorrelationId();
    final token = await _accessToken();
    if (token != null && token.isNotEmpty) request.headers['Authorization'] = 'Bearer $token';
    if (body != null) {
      request.headers['Content-Type'] = 'application/json; charset=utf-8';
      request.body = jsonEncode(body);
    }

    final http.Response response;
    try {
      response = await http.Response.fromStream(await _client.send(request).timeout(_timeout));
    } on TimeoutException {
      throw const ApiException(ApiFailure.network);
    } on http.ClientException {
      throw const ApiException(ApiFailure.network);
    }

    final status = response.statusCode;
    if (status >= 200 && status < 300) {
      return ApiResponse(status, _decode(response, failure: true));
    }
    if (status == 401) _onUnauthorized?.call();
    final problem = _decode(response, failure: false);
    final map = problem is Map ? problem : const {};
    throw ApiException.fromStatus(
      status,
      title: map['title'] is String ? map['title'] as String : null,
      code: map['code'] is String ? map['code'] as String : null,
    );
  }

  Object? _decode(http.Response response, {required bool failure}) {
    if (response.bodyBytes.isEmpty) return null;
    final type = response.headers['content-type'] ?? '';
    if (!type.contains('json')) {
      if (failure) throw const ApiException(ApiFailure.invalidResponse);
      return null;
    }
    try {
      return jsonDecode(utf8.decode(response.bodyBytes));
    } on FormatException {
      if (failure) throw const ApiException(ApiFailure.invalidResponse);
      return null;
    }
  }
}
