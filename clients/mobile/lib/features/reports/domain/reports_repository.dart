import '../../../api/generated/sgt_api.g.dart';

/// Günlük casino özeti (R1).
abstract interface class ReportsRepository {
  /// [date] null ise sunucu bugünü (KKTC iş günü) döndürür.
  Future<DailySummary> daily(DateTime? date);
}
