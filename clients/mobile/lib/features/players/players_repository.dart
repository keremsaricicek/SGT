import '../../api/generated/sgt_api.g.dart';

/// Oyuncu arama (O1) ve konum (O2) verisi.
class PlayersRepository {
  const PlayersRepository(this._api);

  final SgtApiClient _api;

  Future<List<PlayerSummary>> search(String query) => _api.getPlayers(q: query);

  /// Oyuncu şu an bir masada oynamıyorsa null (API 204).
  Future<PlayerLocationDto?> location(String playerId) => _api.getFloorPlayersByPlayerIdLocation(playerId: playerId);
}
