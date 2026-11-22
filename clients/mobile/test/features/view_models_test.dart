import 'package:decimal/decimal.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sgt_mobile/access/access_controller.dart';
import 'package:sgt_mobile/access/user_access.dart';
import 'package:sgt_mobile/api/api_exception.dart';
import 'package:sgt_mobile/api/generated/sgt_api.g.dart';
import 'package:sgt_mobile/features/floor/data/api_floor_repository.dart';
import 'package:sgt_mobile/features/floor/presentation/floor_view_models.dart';
import 'package:sgt_mobile/features/loyalty/presentation/comp_request_view_model.dart';
import 'package:sgt_mobile/features/loyalty/data/api_loyalty_repository.dart';
import 'package:sgt_mobile/features/players/presentation/player_search_view_model.dart';
import 'package:sgt_mobile/features/players/presentation/player_view_model.dart';
import 'package:sgt_mobile/features/players/data/api_players_repository.dart';
import 'package:sgt_mobile/features/reports/data/api_reports_repository.dart';
import 'package:sgt_mobile/features/reports/presentation/daily_summary_view_model.dart';
import 'package:sgt_mobile/shared/load_state.dart';

import '../support/fake_transport.dart';
import '../support/sample_data.dart' as s;

UserAccess accessOf(List<Map<String, Object>> menu) => UserAccess.fromMenu([for (final m in menu) MenuItem.fromJson(m)]);

void main() {
  late FakeTransport t;
  late SgtApiClient api;

  setUp(() {
    t = FakeTransport();
    api = SgtApiClient(t);
  });

  group('Menü / yetki (KK3)', () {
    test('menü yanıtı ekran ve eylemlere çevrilir', () async {
      t.reply('GET', '/api/v1/identity/me/menu', s.menuAll);
      final c = AccessController(api);
      await c.load();
      final a = c.access;
      expect(a.canView('player.search'), isTrue);
      expect(a.can('loyalty.comp', 'request'), isTrue);
      expect(a.can('loyalty.comp', 'approve'), isFalse);
      expect(a.canView('admin.users'), isFalse);
      expect(a.canPermission('cage.shift:open'), isTrue);
    });

    test('menü yüklenemezse hiçbir ekran görünmez', () async {
      t.fail('GET', '/api/v1/identity/me/menu', ApiFailure.network);
      final c = AccessController(api);
      await c.load();
      expect(c.state, isA<Failed<UserAccess>>());
      expect(c.access.screens, isEmpty);
    });
  });

  group('Oyuncu arama (KK6)', () {
    test('2 karakterden kısa aramada istek atılmaz', () async {
      final vm = PlayerSearchViewModel(ApiPlayersRepository(api));
      await vm.search(' d ');
      expect(t.sent, isEmpty);
      expect(vm.tooShort, isTrue);
    });

    test('sonuçlar listelenir, sorgu kırpılır', () async {
      t.on('GET', '/api/v1/players', (q, _) => q['q'] == 'den' ? s.players : const []);
      final vm = PlayerSearchViewModel(ApiPlayersRepository(api));
      await vm.search(' den ');
      final r = vm.results as Loaded<List<PlayerSummary>>;
      expect(r.value.map((p) => p.playerNumber), ['P-004127', 'P-004188', 'P-005310']);
    });

    test('yetki hatası Failed(forbidden) olur', () async {
      t.fail('GET', '/api/v1/players', ApiFailure.forbidden);
      final vm = PlayerSearchViewModel(ApiPlayersRepository(api));
      await vm.search('deniz');
      expect((vm.results as Failed).error.kind, ApiFailure.forbidden);
    });
  });

  group('Oyuncu: konum ve puan (KK7, KK8)', () {
    PlayerViewModel vm(UserAccess access) =>
        PlayerViewModel(playerId: s.p1, players: ApiPlayersRepository(api), loyalty: ApiLoyaltyRepository(api), access: access);

    test('oyundaysa masa kodu ve başlangıç; puan bakiyesi ve hareketler', () async {
      t.reply('GET', '/api/v1/floor/players/${s.p1}/location', s.location);
      t.reply('GET', '/api/v1/loyalty/players/${s.p1}', s.loyalty);
      final m = vm(accessOf(s.menuAll));
      await m.load();
      final loc = (m.location as Loaded<PlayerLocationDto?>).value!;
      expect(loc.tableCode, 'BJ-03');
      expect(loc.since, DateTime.utc(2026, 11, 21, 17, 40));
      final l = (m.loyalty as Loaded<LoyaltySummary>).value;
      expect(l.balance, 18450);
      expect(l.lifetimeEarned, 42310);
      expect(l.recentEntries, hasLength(4));
      expect(m.canRequestComp, isTrue);
    });

    test('204 = şu an oyunda değil', () async {
      t.reply('GET', '/api/v1/floor/players/${s.p1}/location', null);
      final m = vm(accessOf([
        {'screen': 'player.locator', 'actions': ['view']},
      ]));
      await m.load();
      expect((m.location as Loaded<PlayerLocationDto?>).value, isNull);
    });

    test('yetkisi olmayan bölüm için istek atılmaz', () async {
      t.reply('GET', '/api/v1/floor/players/${s.p1}/location', s.location);
      final m = vm(accessOf([
        {'screen': 'player.locator', 'actions': ['view']},
      ]));
      await m.load();
      expect(m.showLoyalty, isFalse);
      expect(m.canRequestComp, isFalse);
      expect(t.count('GET', '/api/v1/loyalty/players/${s.p1}'), 0);
    });
  });

  group('Comp talebi (KK9)', () {
    late int ids;
    CompRequestViewModel vm() {
      ids = 0;
      return CompRequestViewModel(playerId: s.p1, repository: ApiLoyaltyRepository(api), newId: () => 'req-${++ids}');
    }

    test('zorunlu alanlar ve pozitif puan doğrulanır, istek atılmaz', () async {
      final m = vm()..setPoints('-5');
      await m.submit();
      expect(m.errors, {
        CompField.points: CompFieldError.pointsPositive,
        CompField.text: CompFieldError.required,
        CompField.reason: CompFieldError.required,
      });
      expect(t.sent, isEmpty);
    });

    test('gerekçe 50 karakteri aşamaz', () async {
      final m = vm()
        ..setPoints('100')
        ..setText('Yemek')
        ..setReason('X' * 51);
      await m.submit();
      expect(m.errors[CompField.reason], CompFieldError.reasonLength);
    });

    test('gönderim: gerekçe büyük harfe çevrilir, istemci requestId taşır', () async {
      t.reply('POST', '/api/v1/loyalty/comps', {'id': 'req-1'});
      final m = vm()
        ..setPoints(' 1500 ')
        ..setText('Akşam yemeği (2 kişi)')
        ..setReason(' comp_yemek ');
      await m.submit();
      expect(m.state, isA<CompSent>());
      final body = t.sent.single.body as Map;
      expect(body, {
        'requestId': 'req-1',
        'playerId': s.p1,
        'points': 1500,
        'text': 'Akşam yemeği (2 kişi)',
        'reasonCode': 'COMP_YEMEK',
      });
    });

    test('ağ hatasından sonra aynı bilgilerle tekrar: aynı requestId (çift talep yok)', () async {
      t.fail('POST', '/api/v1/loyalty/comps', ApiFailure.network);
      final m = vm()
        ..setPoints('300')
        ..setText('İçecek')
        ..setReason('COMP');
      await m.submit();
      expect((m.state as CompFailed).retrySafe, isTrue);
      t.reply('POST', '/api/v1/loyalty/comps', {'id': 'req-1'});
      await m.submit();
      expect(t.sent.map((r) => (r.body as Map)['requestId']), ['req-1', 'req-1']);
    });

    test('bilgi değişirse yeni requestId üretilir', () async {
      t.fail('POST', '/api/v1/loyalty/comps', ApiFailure.network);
      final m = vm()
        ..setPoints('300')
        ..setText('İçecek')
        ..setReason('COMP');
      await m.submit();
      m.setPoints('350');
      await m.submit();
      expect(t.sent.map((r) => (r.body as Map)['requestId']), ['req-1', 'req-2']);
    });
  });

  group('Günlük özet (KK10)', () {
    test('tarih verilmezse parametre gönderilmez; seçilen tarih yyyy-MM-dd gider', () async {
      t.reply('GET', '/api/v1/reports/daily', s.daily);
      final vm = DailySummaryViewModel(ApiReportsRepository(api));
      await vm.load();
      expect(t.sent.last.query['date'], isNull);
      final d = (vm.summary as Loaded<DailySummary>).value;
      expect(d.cage.currencies.first.chipSales, Decimal.parse('1845000'));
      expect(d.floor.live.last.casinoResult, Decimal.parse('-2300'));

      await vm.load(DateTime(2026, 11, 3));
      expect(t.sent.last.query['date'], '2026-11-03');
    });
  });

  group('Masa ve slot (KK11, KK12)', () {
    test('masalar açık önce sıralanır, süren rating sayısı masaya eşlenir', () async {
      t.reply('GET', '/api/v1/floor/tables', s.tables());
      t.reply('GET', '/api/v1/floor/ratings/running', s.runningRatings());
      final vm = TablesViewModel(ApiFloorRepository(api), canSeeRatings: true);
      await vm.load();
      final o = (vm.overview as Loaded<TablesOverview>).value;
      expect(o.rows.map((r) => r.table.code), ['BC-01', 'BJ-01', 'BJ-02', 'BJ-03', 'RL-01', 'PK-01', 'RL-02']);
      expect(o.rows.firstWhere((r) => r.table.code == 'RL-01').runningRatings, 3);
      expect(o.openCount, 5);
    });

    test('rating yetkisi yoksa rating ucu çağrılmaz', () async {
      t.reply('GET', '/api/v1/floor/tables', s.tables());
      final vm = TablesViewModel(ApiFloorRepository(api), canSeeRatings: false);
      await vm.load();
      expect(t.count('GET', '/api/v1/floor/ratings/running'), 0);
      expect((vm.overview as Loaded<TablesOverview>).value.rows.first.runningRatings, isNull);
    });

    test('slotlar alana göre gruplanır, toplamlar Decimal', () async {
      t.reply('GET', '/api/v1/floor/slots', s.slots());
      final vm = SlotsViewModel(ApiFloorRepository(api));
      await vm.load();
      final areas = (vm.areas as Loaded<List<SlotArea>>).value;
      expect(areas.map((a) => a.name), ['Salon A', 'Salon B (VIP)']);
      expect(areas.first.totals['TRY']!.coinIn, Decimal.parse('102885'));
      expect(areas.first.totals['TRY']!.win, Decimal.parse('4785.75'));
    });
  });
}
