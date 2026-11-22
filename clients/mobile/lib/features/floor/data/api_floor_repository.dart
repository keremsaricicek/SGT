import '../../../api/generated/sgt_api.g.dart';
import '../domain/floor_repository.dart';

class ApiFloorRepository implements FloorRepository {
  const ApiFloorRepository(this._api);

  final SgtApiClient _api;

  @override
  Future<List<TableView>> tables() => _api.getFloorTables();

  @override
  Future<List<RunningRatingView>> runningRatings() => _api.getFloorRatingsRunning();

  @override
  Future<List<SlotFloorRow>> slots() => _api.getFloorSlots();
}
