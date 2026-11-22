import '../../../api/generated/sgt_api.g.dart';
import '../domain/players_repository.dart';

class ApiPlayersRepository implements PlayersRepository {
  const ApiPlayersRepository(this._api);

  final SgtApiClient _api;

  @override
  Future<List<PlayerSummary>> search(String query) => _api.getPlayers(q: query);

  @override
  Future<PlayerLocationDto?> location(String playerId) => _api.getFloorPlayersByPlayerIdLocation(playerId: playerId);
}
