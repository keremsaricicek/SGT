import '../../api/generated/sgt_api.g.dart';

/// Puan (L1) ve comp talebi (L2).
class LoyaltyRepository {
  const LoyaltyRepository(this._api);

  final SgtApiClient _api;

  Future<LoyaltySummary> summary(String playerId) => _api.getLoyaltyPlayersByPlayerId(playerId: playerId);

  Future<CreatedId> requestComp(PointsRequest request) => _api.postLoyaltyComps(body: request);
}
