import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

/// K2: her özellik yalnızca kendi klasörünü ve ortak katmanları (core, shared, ui, api, access, theme, l10n) kullanır.
/// Özellikler arası iletişim `lib/core` sözleşmeleriyle olur ve uygulama düzeyinde (lib/app) bağlanır.
/// Katmanlar: presentation → domain; data → domain; domain → yalnız api sözleşmesi ve core.
void main() {
  final features = Directory('lib/features').listSync().whereType<Directory>().toList();
  final importPattern = RegExp(r"""^import '([^']+)';""", multiLine: true);

  String featureOf(String path) => path.split('/features/')[1].split('/').first;

  test('özellik klasörleri {data,domain,presentation} katmanlarından oluşur', () {
    for (final f in features) {
      for (final entity in f.listSync()) {
        expect(entity, isA<Directory>(), reason: '${entity.path}: dosyalar bir katman klasöründe olmalı');
        expect(['data', 'domain', 'presentation'], contains(entity.path.split('/').last), reason: entity.path);
      }
    }
  });

  test('özellikler birbirini import etmez; presentation data katmanını bilmez', () {
    final violations = <String>[];
    for (final file in Directory('lib/features').listSync(recursive: true).whereType<File>()) {
      if (!file.path.endsWith('.dart')) continue;
      final own = featureOf(file.path);
      final layer = file.path.split('/features/$own/')[1].split('/').first;
      for (final m in importPattern.allMatches(file.readAsStringSync())) {
        final target = m[1]!;
        if (target.startsWith('package:') && !target.startsWith('package:sgt_mobile/')) continue;
        final resolved = target.startsWith('package:sgt_mobile/')
            ? 'lib/${target.substring('package:sgt_mobile/'.length)}'
            : File('${file.parent.path}/$target').uri.normalizePath().path;
        if (resolved.contains('/features/')) {
          final other = featureOf(resolved);
          if (other != own) violations.add('${file.path} → $target (başka özellik)');
          if (layer == 'presentation' && resolved.contains('/features/$own/data/')) {
            violations.add('${file.path} → $target (presentation data katmanını kullanamaz)');
          }
        }
        if (layer == 'domain' && !(resolved.contains('/api/generated/') || resolved.contains('/core/') || resolved.contains('/domain/'))) {
          violations.add('${file.path} → $target (domain yalnız sözleşmeye bağlıdır)');
        }
      }
    }
    expect(violations, isEmpty);
  });
}
