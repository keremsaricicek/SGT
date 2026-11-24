import 'package:flutter/foundation.dart';

import '../api/generated/sgt_api.g.dart';
import '../shared/load_state.dart';
import 'user_access.dart';

/// Oturum açıldıktan sonra menüyü (yetkileri) yükler ve paylaşır.
class AccessController extends ChangeNotifier {
  AccessController(this._api);

  final SgtApiClient _api;
  LoadState<UserAccess> _state = const Idle();

  LoadState<UserAccess> get state => _state;

  UserAccess get access => switch (_state) {
        Loaded(:final value) => value,
        _ => UserAccess.none,
      };

  Future<void> load() async {
    _state = const Loading();
    notifyListeners();
    _state = await runLoad(() async => UserAccess.fromMenu(await _api.getIdentityMeMenu()));
    notifyListeners();
  }

  void clear() {
    _state = const Idle();
    notifyListeners();
  }
}
