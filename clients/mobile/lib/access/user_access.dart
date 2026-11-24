import '../api/generated/sgt_api.g.dart';

/// Kullanıcının ekran yetkileri (`GET /api/v1/identity/me/menu`). Kod biçimi `<ekran>:<işlem>`.
/// Bu yalnızca arayüz kolaylığıdır; asıl kontrol API'dedir.
class UserAccess {
  UserAccess(Map<String, Set<String>> actions) : _actions = Map.unmodifiable(actions);

  factory UserAccess.fromMenu(List<MenuItem> menu) =>
      UserAccess({for (final m in menu) m.screen: {...m.actions}});

  static final none = UserAccess(const {});

  final Map<String, Set<String>> _actions;

  /// Menüde olan ekran görülebilir.
  bool canView(String screen) => _actions.containsKey(screen);

  bool can(String screen, String action) => _actions[screen]?.contains(action) ?? false;

  bool canPermission(String permission) {
    final i = permission.indexOf(':');
    return i > 0 && can(permission.substring(0, i), permission.substring(i + 1));
  }

  Iterable<String> get screens => _actions.keys;
}
