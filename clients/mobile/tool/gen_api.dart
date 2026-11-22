// Kullanım (clients/mobile içinden): dart run tool/gen_api.dart
// docs/api/openapi-v1.json → lib/api/generated/sgt_api.g.dart
// Çıktı biçimlendirilmez; test (test/api/generated_api_sync_test.dart) dosyanın bu çıktıyla birebir aynı olduğunu denetler.
import 'dart:io';

import 'openapi_dart_generator.dart';

const contractPath = '../../docs/api/openapi-v1.json';
const outputPath = 'lib/api/generated/sgt_api.g.dart';

Future<void> main() async {
  final code = generateDartApi(await File(contractPath).readAsString());
  await File(outputPath).create(recursive: true);
  await File(outputPath).writeAsString(code);
  stdout.writeln('Yazıldı: $outputPath');
}
