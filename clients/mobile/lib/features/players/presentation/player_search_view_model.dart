import 'package:flutter/foundation.dart';

import '../../../api/generated/sgt_api.g.dart';
import '../../../shared/load_state.dart';
import '../domain/players_repository.dart';

/// Oyuncu arama (O1, KK6). En az 2 karakter; daha kısa aramada istek atılmaz.
class PlayerSearchViewModel extends ChangeNotifier {
  PlayerSearchViewModel(this._repository);

  static const minLength = 2;

  final PlayersRepository _repository;
  LoadState<List<PlayerSummary>> _results = const Idle();
  bool _tooShort = false;
  int _generation = 0;

  LoadState<List<PlayerSummary>> get results => _results;

  bool get tooShort => _tooShort;

  Future<void> search(String query) async {
    final q = query.trim();
    final generation = ++_generation;
    if (q.length < minLength) {
      _tooShort = q.isNotEmpty;
      _results = const Idle();
      notifyListeners();
      return;
    }
    _tooShort = false;
    _results = const Loading();
    notifyListeners();
    final result = await runLoad(() => _repository.search(q));
    if (generation != _generation) return; // daha yeni bir arama başladı
    _results = result;
    notifyListeners();
  }
}
