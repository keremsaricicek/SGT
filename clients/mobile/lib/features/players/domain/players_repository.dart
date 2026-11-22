import '../../../api/generated/sgt_api.g.dart';

/// Oyuncu arama (O1) ve konum (O2).
abstract interface class PlayersRepository {
  Future<List<PlayerSummary>> search(String query);

  /// Oyuncu şu an bir masada oynamıyorsa null.
  Future<PlayerLocationDto?> location(String playerId);
}
