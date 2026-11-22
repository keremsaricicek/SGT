import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

import '../../tool/openapi_dart_generator.dart';

/// Üretilen istemci sözleşmeyle aynı olmalı (KK14). Kırılırsa: `dart run tool/gen_api.dart`.
void main() {
  test('lib/api/generated/sgt_api.g.dart docs/api/openapi-v1.json ile güncel', () {
    final contract = File('../../docs/api/openapi-v1.json').readAsStringSync();
    final generated = File('lib/api/generated/sgt_api.g.dart').readAsStringSync();
    expect(generated, generateDartApi(contract));
  });

  test('mobilin kullandığı uçlar sözleşmede var', () {
    final code = File('lib/api/generated/sgt_api.g.dart').readAsStringSync();
    for (final op in const [
      'getIdentityMeMenu',
      'getPlayers',
      'getFloorPlayersByPlayerIdLocation',
      'getLoyaltyPlayersByPlayerId',
      'postLoyaltyComps',
      'postLoyaltyCompsByRequestIdDecision',
      'getReportsDaily',
      'getFloorTables',
      'getFloorRatingsRunning',
      'getFloorSlots',
    ]) {
      expect(code, contains('Future<'), reason: op);
      expect(code, contains(' $op('), reason: op);
    }
  });
}
