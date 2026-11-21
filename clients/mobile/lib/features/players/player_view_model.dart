import 'package:flutter/foundation.dart';

import '../../access/screen_codes.dart';
import '../../access/user_access.dart';
import '../../api/generated/sgt_api.g.dart';
import '../../shared/load_state.dart';
import '../loyalty/loyalty_repository.dart';
import 'players_repository.dart';

/// Seçilen oyuncu: "şu an nerede" (O2, KK7) ve puan (L1, KK8). Yetkisi olmayan bölüm yüklenmez.
class PlayerViewModel extends ChangeNotifier {
  PlayerViewModel({
    required this.playerId,
    required this._players,
    required this._loyalty,
    required UserAccess access,
  })  : showLocation = access.canView(ScreenCodes.playerLocator),
        showLoyalty = access.canView(ScreenCodes.loyaltyPoints),
        canRequestComp = access.can(ScreenCodes.loyaltyComp, ScreenActions.request);

  final String playerId;
  final PlayersRepository _players;
  final LoyaltyRepository _loyalty;
  final bool showLocation;
  final bool showLoyalty;
  final bool canRequestComp;

  /// Loaded(null) = şu an oyunda değil.
  LoadState<PlayerLocationDto?> location = const Idle();
  LoadState<LoyaltySummary> loyalty = const Idle();

  Future<void> load() async {
    if (showLocation) location = const Loading();
    if (showLoyalty) loyalty = const Loading();
    notifyListeners();
    await Future.wait([
      if (showLocation) runLoad(() => _players.location(playerId)).then((r) => location = r),
      if (showLoyalty) runLoad(() => _loyalty.summary(playerId)).then((r) => loyalty = r),
    ]);
    notifyListeners();
  }
}
