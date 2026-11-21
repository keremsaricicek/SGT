import 'package:decimal/decimal.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sgt_mobile/api/api_codec.dart';
import 'package:sgt_mobile/api/generated/sgt_api.g.dart';

void main() {
  group('ApiCodec', () {
    test('para alanı sayı veya metin olarak gelse de Decimal okunur (double yok, KK13)', () {
      expect(ApiCodec.decimal(1250.75), Decimal.parse('1250.75'));
      expect(ApiCodec.decimal('0.10'), Decimal.parse('0.1'));
      expect(ApiCodec.decimal(3), Decimal.fromInt(3));
    });

    test('int64 alanı sayı veya metin olabilir', () {
      expect(ApiCodec.integer(42), 42);
      expect(ApiCodec.integer('9007199254740993'), 9007199254740993);
    });

    test('tarih ve zaman UTC okunur, tarih yyyy-MM-dd yazılır', () {
      final at = ApiCodec.dateTime('2026-11-21T08:30:00Z');
      expect(at.isUtc, isTrue);
      expect(at.hour, 8);
      expect(ApiCodec.writeDate(DateTime(2026, 11, 5)), '2026-11-05');
      expect(ApiCodec.date('2026-11-05'), DateTime.utc(2026, 11, 5));
    });

    test('beklenmeyen biçim FormatException verir', () {
      expect(() => ApiCodec.string(5), throwsFormatException);
      expect(() => ApiCodec.object([1]), throwsFormatException);
      expect(() => ApiCodec.decimal(true), throwsFormatException);
    });
  });

  group('üretilen DTO', () {
    test('PlayerSummary JSON okur ve yazar; boş kart no null kalır', () {
      final p = PlayerSummary.fromJson({
        'playerId': '6f1c1a5e-0000-4000-8000-000000000001',
        'playerNumber': 'P-000123',
        'firstName': 'Deniz',
        'lastName': 'Örnek',
        'activeCardNumber': null,
        'isRestricted': false,
      });
      expect(p.playerNumber, 'P-000123');
      expect(p.activeCardNumber, isNull);
      expect(p.toJson()['lastName'], 'Örnek');
    });

    test('TableView limitleri Decimal, enum dışı oyun türü metin olarak kalır', () {
      final t = TableView.fromJson({
        'tableId': '6f1c1a5e-0000-4000-8000-000000000002',
        'code': 'BJ-01',
        'gameType': 'Blackjack',
        'currency': 'EUR',
        'minBet': '25',
        'maxBet': 2500,
        'houseEdgePercent': 0.5,
        'handsPerHour': '60',
        'isOpen': true,
      });
      expect(t.minBet, Decimal.fromInt(25));
      expect(t.houseEdgePercent, Decimal.parse('0.5'));
      expect(t.handsPerHour, 60);
    });

    test('PointsRequest puanı int olarak, tutarları metin olarak gönderir', () {
      final body = const PointsRequest(
        requestId: 'r',
        playerId: 'p',
        points: 500,
        text: 'Akşam yemeği',
        reasonCode: 'COMP',
      ).toJson();
      expect(body['points'], 500);
    });

    test('string enum wire değeriyle eşleşir', () {
      expect(GameType.fromJson('Roulette'), GameType.roulette);
      expect(GameType.baccarat.toJson(), 'Baccarat');
      expect(() => GameType.fromJson('Craps'), throwsFormatException);
    });
  });
}
