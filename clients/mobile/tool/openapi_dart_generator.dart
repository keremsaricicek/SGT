/// OpenAPI 3.1 sözleşmesinden (docs/api/openapi-v1.json) Dart DTO ve istemci kodu üretir.
///
/// Elle DTO yazılmaz (Spec 014 KK14). Desteklenen alt küme sözleşmede kullanılanlardır:
/// nesne şemaları, string enum'lar, `$ref`, diziler, `["null", T]` ve .NET'in sayı/metin
/// birleşimleri (`["number","string"]` → Decimal, `["integer","string"]` → int).
library;

import 'dart:convert';

const _header = '// GENERATED CODE - DO NOT MODIFY BY HAND.\n'
    '// Kaynak: docs/api/openapi-v1.json · Üretici: tool/gen_api.dart\n'
    '// Sözleşme değişince: dart run tool/gen_api.dart\n';

const _reserved = {
  'assert', 'break', 'case', 'catch', 'class', 'const', 'continue', 'default', 'do', 'else', 'enum',
  'extends', 'false', 'final', 'finally', 'for', 'if', 'in', 'is', 'new', 'null', 'rethrow', 'return',
  'super', 'switch', 'this', 'throw', 'true', 'try', 'var', 'void', 'while', 'with',
};

/// Sözleşme metnini Dart kaynak koduna çevirir. Saf fonksiyondur; test aynı çıktıyı dosyayla karşılaştırır.
String generateDartApi(String openApiJson) {
  final doc = jsonDecode(openApiJson) as Map<String, Object?>;
  final schemas = ((doc['components'] as Map?)?['schemas'] as Map?)?.cast<String, Object?>() ?? {};
  final paths = (doc['paths'] as Map).cast<String, Object?>();
  final out = StringBuffer()
    ..writeln(_header)
    ..writeln('// ignore_for_file: public_member_api_docs, constant_identifier_names')
    ..writeln()
    ..writeln("import 'package:decimal/decimal.dart';")
    ..writeln()
    ..writeln("import '../api_codec.dart';")
    ..writeln("import '../api_transport.dart';")
    ..writeln();

  final names = schemas.keys.toList()..sort();
  for (final name in names) {
    final schema = (schemas[name] as Map).cast<String, Object?>();
    if (schema['enum'] is List) {
      _writeEnum(out, name, (schema['enum'] as List).cast<String>());
    } else {
      _writeClass(out, name, schema);
    }
  }
  _writeClient(out, paths);
  return out.toString();
}

void _writeEnum(StringBuffer out, String name, List<String> values) {
  out.writeln('enum $name {');
  for (final v in values) {
    out.writeln("  ${_ident(_lowerFirst(v))}('$v'),");
  }
  out
    ..writeln('  ;')
    ..writeln()
    ..writeln('  const $name(this.wire);')
    ..writeln()
    ..writeln('  final String wire;')
    ..writeln()
    ..writeln('  static $name fromJson(Object? value) => ApiCodec.enumValue(value, values, (e) => e.wire);')
    ..writeln()
    ..writeln('  String toJson() => wire;')
    ..writeln('}')
    ..writeln();
}

void _writeClass(StringBuffer out, String name, Map<String, Object?> schema) {
  final props = (schema['properties'] as Map?)?.cast<String, Object?>() ?? {};
  final required = ((schema['required'] as List?) ?? const []).cast<String>().toSet();
  final fields = [
    for (final e in props.entries)
      _Field(e.key, _typeOf((e.value as Map).cast<String, Object?>(), nullable: !required.contains(e.key))),
  ];

  out.writeln('class $name {');
  if (fields.isEmpty) {
    out.writeln('  const $name();');
  } else {
    out.writeln('  const $name({');
    for (final f in fields) {
      out.writeln('    ${f.type.nullable ? '' : 'required '}this.${f.ident},');
    }
    out.writeln('  });');
  }
  out.writeln();
  for (final f in fields) {
    out.writeln('  final ${f.type.dart} ${f.ident};');
  }
  if (fields.isNotEmpty) out.writeln();
  out.writeln('  factory $name.fromJson(Object? json) {');
  if (fields.isEmpty) {
    out
      ..writeln('    ApiCodec.object(json);')
      ..writeln('    return const $name();');
  } else {
    out
      ..writeln('    final m = ApiCodec.object(json);')
      ..writeln('    return $name(');
    for (final f in fields) {
      out.writeln("      ${f.ident}: ${f.type.read("m['${f.wire}']")},");
    }
    out.writeln('    );');
  }
  out
    ..writeln('  }')
    ..writeln()
    ..writeln('  Map<String, Object?> toJson() => {');
  for (final f in fields) {
    out.writeln("        '${f.wire}': ${f.type.write(f.ident)},");
  }
  out
    ..writeln('      };')
    ..writeln('}')
    ..writeln();
}

void _writeClient(StringBuffer out, Map<String, Object?> paths) {
  out
    ..writeln('/// Tipli API istemcisi. Her metot bir uca karşılık gelir; ad = HTTP metodu + yol.')
    ..writeln('class SgtApiClient {')
    ..writeln('  const SgtApiClient(this._transport);')
    ..writeln()
    ..writeln('  final ApiTransport _transport;');
  final sortedPaths = paths.keys.where((p) => p.startsWith('/api/v1/')).toList()..sort();
  for (final path in sortedPaths) {
    final ops = (paths[path] as Map).cast<String, Object?>();
    for (final method in const ['get', 'post', 'put', 'delete', 'patch']) {
      final op = ops[method];
      if (op is Map) _writeOperation(out, method, path, op.cast<String, Object?>());
    }
  }
  out.writeln('}');
}

void _writeOperation(StringBuffer out, String method, String path, Map<String, Object?> op) {
  final params = ((op['parameters'] as List?) ?? const []).cast<Map>().map((p) => p.cast<String, Object?>()).toList();
  final pathParams = params.where((p) => p['in'] == 'path').toList();
  final queryParams = params.where((p) => p['in'] == 'query').toList();
  final body = op['requestBody'] as Map?;
  final bodyType = body == null
      ? null
      : _typeOf((((body['content'] as Map)['application/json'] as Map)['schema'] as Map).cast<String, Object?>(),
          nullable: false);
  final responses = (op['responses'] as Map).cast<String, Object?>();
  _DartType? result;
  for (final code in const ['200', '201']) {
    final content = (responses[code] as Map?)?['content'] as Map?;
    final json = content?['application/json'] as Map?;
    if (json != null) {
      result = _typeOf((json['schema'] as Map).cast<String, Object?>(), nullable: responses.containsKey('204'));
      break;
    }
  }

  final name = _operationName(method, path);
  final args = <String>[
    for (final p in pathParams) 'required ${_typeOf(_schemaOf(p), nullable: false).dart} ${_ident(p['name'] as String)}',
    if (bodyType != null) 'required ${bodyType.dart} body',
    for (final p in queryParams) '${_typeOf(_schemaOf(p), nullable: true).dart} ${_ident(p['name'] as String)}',
  ];
  final returnType = result == null ? 'void' : result.dart;
  final dartPath = path.replaceAllMapped(RegExp(r'\{(\w+)\}'), (m) => '\${Uri.encodeComponent(${_ident(m[1]!)}.toString())}');

  out
    ..writeln()
    ..writeln('  /// ${method.toUpperCase()} $path')
    ..writeln('  Future<$returnType> $name(${args.isEmpty ? '' : '{${args.join(', ')}}'}) async {')
    ..write("    ${result == null ? '' : 'final response = '}await _transport.send('${method.toUpperCase()}', '$dartPath'");
  if (queryParams.isNotEmpty) {
    final q = queryParams.map((p) {
      final t = _typeOf(_schemaOf(p), nullable: true);
      return "'${p['name']}': ${t.query(_ident(p['name'] as String))}";
    }).join(', ');
    out.write(', query: {$q}');
  }
  if (bodyType != null) out.write(', body: ${bodyType.write('body')}');
  out.writeln(');');
  if (result != null) {
    out.writeln('    return ApiCodec.decode(() => ${result.read('response.body')});');
  }
  out.writeln('  }');
}

Map<String, Object?> _schemaOf(Map<String, Object?> param) => (param['schema'] as Map).cast<String, Object?>();

String _operationName(String method, String path) {
  final segments = path.substring('/api/v1/'.length).split('/');
  final parts = <String>[method];
  for (final s in segments) {
    final param = RegExp(r'^\{(\w+)\}$').firstMatch(s);
    parts.add(param == null ? _pascal(s) : 'By${_pascal(param[1]!)}');
  }
  return parts.join();
}

String _pascal(String s) => s
    .split(RegExp(r'[-_]'))
    .where((p) => p.isNotEmpty)
    .map((p) => p[0].toUpperCase() + p.substring(1))
    .join();

String _lowerFirst(String s) => s[0].toLowerCase() + s.substring(1);

String _ident(String s) => _reserved.contains(s) ? '$s\$' : s;

class _Field {
  _Field(this.wire, this.type);
  final String wire;
  final _DartType type;
  String get ident => _ident(wire);
}

/// Bir şema düğümünün Dart karşılığı ve okuma/yazma ifadeleri.
class _DartType {
  _DartType(this.base, this.nullable, this._reader, this._writer, [this._query]);

  final String base;
  final bool nullable;
  final String Function(String) _reader;
  final String Function(String) _writer;
  final String Function(String)? _query;

  String get dart => nullable ? '$base?' : base;

  String read(String expr) => nullable ? 'ApiCodec.nullable($expr, (v) => ${_reader('v')})' : _reader(expr);

  String write(String expr) =>
      nullable ? (_isIdentity ? expr : 'ApiCodec.nullable($expr, (v) => ${_writer('v')})') : _writer(expr);

  String query(String expr) {
    final q = _query ?? (e) => '${_writer(e)}.toString()';
    return q('v') == 'v' ? expr : 'ApiCodec.nullable($expr, (v) => ${q('v')})';
  }

  bool get _isIdentity => _writer('x') == 'x';
}

_DartType _typeOf(Map<String, Object?> schema, {required bool nullable}) {
  final ref = schema[r'$ref'] as String?;
  if (ref != null) {
    final name = ref.split('/').last;
    return _DartType(name, nullable, (e) => '$name.fromJson($e)', (e) => '$e.toJson()');
  }
  final rawType = schema['type'];
  final types = rawType is List ? rawType.cast<String>().toSet() : {if (rawType is String) rawType};
  final isNullable = nullable || types.remove('null');
  final format = schema['format'] as String?;

  if (types.contains('array')) {
    final item = _typeOf((schema['items'] as Map).cast<String, Object?>(), nullable: false);
    return _DartType('List<${item.dart}>', isNullable, (e) => 'ApiCodec.list($e, (v) => ${item.read('v')})',
        (e) => '$e.map((v) => ${item.write('v')}).toList()');
  }
  if (types.contains('number')) {
    return _DartType('Decimal', isNullable, (e) => 'ApiCodec.decimal($e)', (e) => '$e.toString()');
  }
  if (types.contains('integer')) {
    return _DartType('int', isNullable, (e) => 'ApiCodec.integer($e)', (e) => e);
  }
  if (types.contains('boolean')) {
    return _DartType('bool', isNullable, (e) => 'ApiCodec.boolean($e)', (e) => e);
  }
  if (types.contains('string')) {
    switch (format) {
      case 'date-time':
        return _DartType('DateTime', isNullable, (e) => 'ApiCodec.dateTime($e)', (e) => 'ApiCodec.writeDateTime($e)');
      case 'date':
        return _DartType('DateTime', isNullable, (e) => 'ApiCodec.date($e)', (e) => 'ApiCodec.writeDate($e)');
      default:
        return _DartType('String', isNullable, (e) => 'ApiCodec.string($e)', (e) => e, (e) => e);
    }
  }
  throw UnsupportedError('Desteklenmeyen şema: ${jsonEncode(schema)}');
}
