import '../../../api/generated/sgt_api.g.dart';
import '../domain/reports_repository.dart';

class ApiReportsRepository implements ReportsRepository {
  const ApiReportsRepository(this._api);

  final SgtApiClient _api;

  @override
  Future<DailySummary> daily(DateTime? date) => _api.getReportsDaily(date: date);
}
