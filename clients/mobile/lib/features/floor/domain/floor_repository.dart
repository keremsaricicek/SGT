import '../../../api/generated/sgt_api.g.dart';

/// Masa (C1) ve slot salonu (S1) verisi; salt okunur.
abstract interface class FloorRepository {
  Future<List<TableView>> tables();

  Future<List<RunningRatingView>> runningRatings();

  Future<List<SlotFloorRow>> slots();
}
