import 'package:decimal/decimal.dart';

import 'api_exception.dart';

/// Üretilen DTO'ların JSON okuma/yazma yardımcıları.
///
/// API (.NET) sayıları sayı veya metin olarak gönderebilir (`["number","string"]`); para her zaman
/// [Decimal] olarak okunur, `double` kullanılmaz (K1, Spec 014 KK13).
abstract final class ApiCodec {
  /// Üretilen bir yanıt okumasını çalıştırır; biçim hatasını [ApiException] (invalidResponse) yapar.
  static T decode<T>(T Function() read) {
    try {
      return read();
    } on FormatException {
      throw const ApiException(ApiFailure.invalidResponse);
    } on TypeError {
      throw const ApiException(ApiFailure.invalidResponse);
    }
  }

  static R? nullable<S extends Object, R>(S? value, R Function(S value) map) => value == null ? null : map(value);

  static Map<String, Object?> object(Object? value) {
    if (value is Map<String, Object?>) return value;
    if (value is Map) return value.cast<String, Object?>();
    throw const FormatException('Nesne bekleniyordu');
  }

  static List<T> list<T>(Object? value, T Function(Object? item) read) {
    if (value is! List) throw const FormatException('Dizi bekleniyordu');
    return List<T>.unmodifiable(value.map(read));
  }

  static String string(Object? value) {
    if (value is String) return value;
    throw const FormatException('Metin bekleniyordu');
  }

  static bool boolean(Object? value) {
    if (value is bool) return value;
    throw const FormatException('Mantıksal değer bekleniyordu');
  }

  static int integer(Object? value) {
    if (value is int) return value;
    if (value is String) return int.parse(value);
    throw const FormatException('Tam sayı bekleniyordu');
  }

  /// IEEE double'ın tam sayıyı birebir taşıdığı sınır (2^53).
  static final _maxExact = BigInt.two.pow(53);

  /// Para okuması. Metin gelirse metin olduğu gibi (kayıpsız) çözülür. JSON sayısı ise `double`'dan geçtiği için
  /// yalnızca birebir geri dönüşüyorsa kabul edilir: üslü gösterim yok ve ölçeklenmiş değer |x| < 2^53.
  /// Aksi halde [FormatException] → yanıt `invalidResponse` sayılır (tutar sessizce yuvarlanmaz).
  static Decimal decimal(Object? value) {
    if (value is String) return Decimal.parse(value);
    if (value is num) {
      final lexeme = value.toString();
      if (lexeme.contains('e') || lexeme.contains('E')) {
        throw const FormatException('Para alanı üslü gösterimle geldi (kayıplı)');
      }
      final d = Decimal.parse(lexeme);
      final dot = lexeme.indexOf('.');
      final scale = dot < 0 ? 0 : lexeme.length - dot - 1;
      final scaled = (d * Decimal.ten.pow(scale).toDecimal()).toBigInt().abs();
      if (scaled >= _maxExact) {
        throw const FormatException('Para alanı double hassasiyetini aşıyor (kayıplı)');
      }
      return d;
    }
    throw const FormatException('Ondalık sayı bekleniyordu');
  }

  static DateTime dateTime(Object? value) => DateTime.parse(string(value)).toUtc();

  /// `yyyy-MM-dd` → gün başı (UTC işaretli; yalnızca takvim günü taşır).
  static DateTime date(Object? value) {
    final d = DateTime.parse(string(value));
    return DateTime.utc(d.year, d.month, d.day);
  }

  static String writeDateTime(DateTime value) => value.toUtc().toIso8601String();

  static String writeDate(DateTime value) =>
      '${value.year.toString().padLeft(4, '0')}-${value.month.toString().padLeft(2, '0')}-${value.day.toString().padLeft(2, '0')}';

  static T enumValue<T>(Object? value, List<T> values, String Function(T) wire) {
    final s = string(value);
    for (final v in values) {
      if (wire(v) == s) return v;
    }
    throw FormatException('Bilinmeyen değer: $s');
  }
}
