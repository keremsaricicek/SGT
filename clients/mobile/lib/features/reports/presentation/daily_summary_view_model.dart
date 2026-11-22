import 'package:flutter/foundation.dart';

import '../../../api/generated/sgt_api.g.dart';
import '../../../shared/load_state.dart';
import '../domain/reports_repository.dart';

/// Günlük casino özeti (R1, KK10).
class DailySummaryViewModel extends ChangeNotifier {
  DailySummaryViewModel(this._repository);

  final ReportsRepository _repository;

  DateTime? _date;
  LoadState<DailySummary> summary = const Idle();

  /// Seçilen takvim günü; null = bugün.
  DateTime? get date => _date;

  Future<void> load([DateTime? date]) async {
    _date = date;
    summary = const Loading();
    notifyListeners();
    summary = await runLoad(() => _repository.daily(date));
    notifyListeners();
  }

  Future<void> refresh() => load(_date);
}
