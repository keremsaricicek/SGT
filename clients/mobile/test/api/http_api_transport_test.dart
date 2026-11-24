import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:sgt_mobile/api/api_exception.dart';
import 'package:sgt_mobile/api/api_transport.dart';
import 'package:sgt_mobile/api/generated/sgt_api.g.dart';

void main() {
  late List<http.Request> sent;
  var unauthorizedCalls = 0;

  HttpApiTransport transport(MockClientHandler handler, {String base = 'https://api.test/'}) {
    sent = [];
    unauthorizedCalls = 0;
    return HttpApiTransport(
      client: MockClient((r) {
        sent.add(r);
        return handler(r);
      }),
      baseUri: Uri.parse(base),
      accessToken: () async => 'tok-123',
      terminalId: 'M-0a1b2c3d',
      newCorrelationId: () => 'c0ffee00-0000-4000-8000-000000000001',
      onUnauthorized: () => unauthorizedCalls++,
    );
  }

  http.Response json(Object body, int status, {String type = 'application/json'}) =>
      http.Response(jsonEncode(body), status, headers: {'content-type': '$type; charset=utf-8'});

  test('her istek bearer, X-Terminal ve X-Correlation-Id taşır (KK4)', () async {
    final api = SgtApiClient(transport((_) async => json([], 200)));
    await api.getPlayers(q: 'den');

    final r = sent.single;
    expect(r.url.toString(), 'https://api.test/api/v1/players?q=den');
    expect(r.headers['Authorization'], 'Bearer tok-123');
    expect(r.headers['X-Terminal'], 'M-0a1b2c3d');
    expect(r.headers['X-Correlation-Id'], 'c0ffee00-0000-4000-8000-000000000001');
  });

  test('boş sorgu parametresi gönderilmez', () async {
    final api = SgtApiClient(transport((_) async => json({
          'date': '2026-11-21',
          'from': '2026-11-20T22:00:00Z',
          'to': '2026-11-21T22:00:00Z',
          'cage': {'shiftsOpen': 0, 'shiftsClosed': 0, 'reversals': 0, 'currencies': []},
          'floor': {'ratingsFinished': 0, 'live': [], 'slots': []},
          'loyalty': {'pointsEarned': 0, 'pointsRedeemed': 0},
        }, 200)));
    await api.getReportsDaily();
    expect(sent.single.url.query, isEmpty);
  });

  test('204 yanıtı null döner (oyuncu oyunda değil, KK7)', () async {
    final api = SgtApiClient(transport((_) async => http.Response('', 204)));
    final location = await api.getFloorPlayersByPlayerIdLocation(playerId: 'p1');
    expect(location, isNull);
  });

  test('POST gövdesi JSON gönderilir', () async {
    final api = SgtApiClient(transport((_) async => json({'id': 'abc'}, 201)));
    await api.postLoyaltyComps(
      body: const PointsRequest(requestId: 'r1', playerId: 'p1', points: 300, text: 'Yemek', reasonCode: 'COMP'),
    );
    final r = sent.single;
    expect(r.method, 'POST');
    expect(r.headers['Content-Type'], startsWith('application/json'));
    expect(jsonDecode(r.body), containsPair('requestId', 'r1'));
  });

  test('problem+json başlık ve kod ApiException olur (KK5)', () async {
    final api = SgtApiClient(transport((_) async => json(
          {'title': 'Yetersiz puan', 'status': 409, 'code': 'Loyalty.InsufficientPoints'},
          409,
          type: 'application/problem+json',
        )));
    await expectLater(
      api.postLoyaltyComps(
        body: const PointsRequest(requestId: 'r', playerId: 'p', points: 1, text: 't', reasonCode: 'C'),
      ),
      throwsA(isA<ApiException>()
          .having((e) => e.kind, 'kind', ApiFailure.conflict)
          .having((e) => e.title, 'title', 'Yetersiz puan')
          .having((e) => e.code, 'code', 'Loyalty.InsufficientPoints')),
    );
  });

  test('401 oturumu kapatma geri çağrısını tetikler', () async {
    final api = SgtApiClient(transport((_) async => http.Response('', 401)));
    await expectLater(api.getIdentityMeMenu(), throwsA(isA<ApiException>().having((e) => e.kind, 'kind', ApiFailure.unauthorized)));
    expect(unauthorizedCalls, 1);
  });

  test('403 yetki hatası olarak sınıflanır', () async {
    final api = SgtApiClient(transport((_) async => http.Response('', 403)));
    await expectLater(api.getFloorTables(), throwsA(isA<ApiException>().having((e) => e.kind, 'kind', ApiFailure.forbidden)));
  });

  test('ağ hatası network olarak sınıflanır', () async {
    final api = SgtApiClient(transport((_) async => throw http.ClientException('bağlantı yok')));
    await expectLater(api.getFloorSlots(), throwsA(isA<ApiException>().having((e) => e.kind, 'kind', ApiFailure.network)));
  });

  test('beklenmeyen yanıt gövdesi invalidResponse olur', () async {
    final api = SgtApiClient(transport((_) async => http.Response('<html>', 200, headers: {'content-type': 'text/html'})));
    await expectLater(api.getFloorSlots(), throwsA(isA<ApiException>().having((e) => e.kind, 'kind', ApiFailure.invalidResponse)));
  });

  test('üretimde http adresi reddedilir (KK17)', () {
    expect(
      () => HttpApiTransport(
        client: MockClient((_) async => http.Response('', 200)),
        baseUri: Uri.parse('http://api.test/'),
        accessToken: () async => null,
        terminalId: 'M-00000000',
        newCorrelationId: () => 'x',
        requireHttps: true,
      ),
      throwsArgumentError,
    );
  });
}
