import '../../../api/generated/sgt_api.g.dart';
import '../domain/loyalty_repository.dart';

class ApiLoyaltyRepository implements LoyaltyRepository {
  const ApiLoyaltyRepository(this._api);

  final SgtApiClient _api;

  @override
  Future<LoyaltySummary> summary(String playerId) => _api.getLoyaltyPlayersByPlayerId(playerId: playerId);

  @override
  Future<CreatedId> requestComp(PointsRequest request) => _api.postLoyaltyComps(body: request);
}
