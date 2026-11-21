import 'package:decimal/decimal.dart';
import 'package:flutter/foundation.dart';

import '../../api/generated/sgt_api.g.dart';
import '../../shared/load_state.dart';

class FloorRepository {
  const FloorRepository(this._api);

  final SgtApiClient _api;

  Future<List<TableView>> tables() => _api.getFloorTables();

  Future<List<RunningRatingView>> runningRatings() => _api.getFloorRatingsRunning();

  Future<List<SlotFloorRow>> slots() => _api.getFloorSlots();
}

/// Masa satırı: masa + üzerinde süren rating sayısı.
class FloorTableRow {
  const FloorTableRow(this.table, this.runningRatings);

  final TableView table;

  /// `live.rating:view` yetkisi yoksa null (gösterilmez).
  final int? runningRatings;
}

class TablesOverview {
  const TablesOverview(this.rows);

  final List<FloorTableRow> rows;

  int get openCount => rows.where((r) => r.table.isOpen).length;
}

/// Masa haritası (C1, KK11). Salt okunur; açık masalar önce, sonra koda göre.
class TablesViewModel extends ChangeNotifier {
  TablesViewModel(this._repository, {required this.canSeeRatings});

  final FloorRepository _repository;
  final bool canSeeRatings;
  LoadState<TablesOverview> overview = const Idle();

  Future<void> load() async {
    overview = const Loading();
    notifyListeners();
    overview = await runLoad(() async {
      final results = await Future.wait<Object>([
        _repository.tables(),
        if (canSeeRatings) _repository.runningRatings(),
      ]);
      final tables = results[0] as List<TableView>;
      final counts = <String, int>{};
      if (canSeeRatings) {
        for (final r in results[1] as List<RunningRatingView>) {
          counts[r.tableId] = (counts[r.tableId] ?? 0) + 1;
        }
      }
      final rows = [
        for (final t in tables) FloorTableRow(t, canSeeRatings ? (counts[t.tableId] ?? 0) : null),
      ]..sort((a, b) {
          if (a.table.isOpen != b.table.isOpen) return a.table.isOpen ? -1 : 1;
          return a.table.code.compareTo(b.table.code);
        });
      return TablesOverview(rows);
    });
    notifyListeners();
  }
}

/// Bir slot alanı: makineler ve para birimi bazında bugünkü toplamlar.
class SlotArea {
  const SlotArea(this.name, this.machines, this.totals);

  final String name;
  final List<SlotFloorRow> machines;

  /// para birimi → (giren, kazanç)
  final Map<String, ({Decimal coinIn, Decimal win})> totals;
}

/// Slot salonu (S1, KK12). Alana göre gruplu, salt okunur.
class SlotsViewModel extends ChangeNotifier {
  SlotsViewModel(this._repository);

  final FloorRepository _repository;
  LoadState<List<SlotArea>> areas = const Idle();

  Future<void> load() async {
    areas = const Loading();
    notifyListeners();
    areas = await runLoad(() async => group(await _repository.slots()));
    notifyListeners();
  }

  static List<SlotArea> group(List<SlotFloorRow> rows) {
    final byArea = <String, List<SlotFloorRow>>{};
    for (final r in rows) {
      byArea.putIfAbsent(r.area, () => []).add(r);
    }
    final names = byArea.keys.toList()..sort();
    return [
      for (final name in names)
        () {
          final machines = byArea[name]!..sort((a, b) => a.assetNumber.compareTo(b.assetNumber));
          final totals = <String, ({Decimal coinIn, Decimal win})>{};
          for (final m in machines) {
            final t = totals[m.currency] ?? (coinIn: Decimal.zero, win: Decimal.zero);
            totals[m.currency] = (coinIn: t.coinIn + m.todayCoinIn, win: t.win + m.todayWin);
          }
          return SlotArea(name, List.unmodifiable(machines), totals);
        }(),
    ];
  }
}
