import 'dart:math';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';

abstract interface class TerminalIdStore {
  Future<String?> read();

  Future<void> write(String id);
}

class SecureTerminalIdStore implements TerminalIdStore {
  const SecureTerminalIdStore([this._storage = const FlutterSecureStorage()]);

  static const _key = 'sgt.terminal_id';
  final FlutterSecureStorage _storage;

  @override
  Future<String?> read() => _storage.read(key: _key);

  @override
  Future<void> write(String id) => _storage.write(key: _key, value: id);
}

/// `X-Terminal` değeri: kuruluma özgü rastgele kimlik (`M-` + 8 hex). Cihaz adı kişisel bilgi
/// içerebileceği için kullanılmaz (Spec 014 KK4). Audit kaydında işlemin hangi telefondan geldiğini ayırt eder.
abstract final class TerminalId {
  static final _pattern = RegExp(r'^M-[0-9a-f]{8}$');

  static Future<String> load(TerminalIdStore store, [Random? random]) async {
    final existing = await store.read();
    if (existing != null && _pattern.hasMatch(existing)) return existing;
    final r = random ?? Random.secure();
    final id = 'M-${List.generate(4, (_) => r.nextInt(256).toRadixString(16).padLeft(2, '0')).join()}';
    await store.write(id);
    return id;
  }
}
