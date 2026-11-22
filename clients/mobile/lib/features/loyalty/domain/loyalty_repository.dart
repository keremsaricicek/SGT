import '../../../api/generated/sgt_api.g.dart';
import '../../../core/loyalty_points.dart';

/// Puan (L1) ve comp talebi (L2). Puan okuma ortak sözleşmedir ([LoyaltyPointsReader]).
abstract interface class LoyaltyRepository implements LoyaltyPointsReader {
  Future<CreatedId> requestComp(PointsRequest request);
}
