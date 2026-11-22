import 'package:sgt_mobile/api/api_exception.dart';
import 'package:sgt_mobile/api/api_transport.dart';

/// Yol → yanıt eşlemesi olan sahte taşıma. Gönderilen istekleri kaydeder.
class FakeTransport implements ApiTransport {
  final Map<String, Object? Function(Map<String, String?> query, Object? body)> _routes = {};
  final List<({String method, String path, Map<String, String?> query, Object? body})> sent = [];

  void on(String method, String path, Object? Function(Map<String, String?> query, Object? body) handler) =>
      _routes['$method $path'] = handler;

  void reply(String method, String path, Object? body) => on(method, path, (_, _) => body);

  void fail(String method, String path, ApiFailure kind, {String? title}) =>
      on(method, path, (_, _) => throw ApiException(kind, title: title));

  int count(String method, String path) => sent.where((r) => r.method == method && r.path == path).length;

  @override
  Future<ApiResponse> send(String method, String path, {Map<String, String?> query = const {}, Object? body}) async {
    sent.add((method: method, path: path, query: query, body: body));
    final handler = _routes['$method $path'];
    if (handler == null) throw StateError('Tanımsız uç: $method $path');
    final result = handler(query, body);
    return ApiResponse(result == null ? 204 : 200, result);
  }
}
