import 'package:decimal/decimal.dart';
import 'package:timezone/timezone.dart' as tz;

/// Sayı, para ve zaman gösterimi. Para [Decimal] ile biçimlenir; `double`'a çevrilmez (KK13).
class SgtFormat {
  const SgtFormat(this.languageCode);

  /// İş takvimi saat dilimi (BusinessCalendar ile aynı; DB'de UTC tutulur).
  static const businessTimeZone = 'Asia/Famagusta';

  final String languageCode;

  bool get _tr => languageCode == 'tr';
  String get _group => _tr ? '.' : ',';
  String get _decimalMark => _tr ? ',' : '.';

  /// `12.345,67 EUR` (tr) / `12,345.67 EUR` (en). 2 ondalık, yarım yukarı yuvarlama.
  String money(Decimal amount, String currency) => '${amountOnly(amount)} $currency';

  String amountOnly(Decimal amount) {
    final fixed = amount.round(scale: 2).toStringAsFixed(2);
    final negative = fixed.startsWith('-');
    final digits = negative ? fixed.substring(1) : fixed;
    final dot = digits.indexOf('.');
    final whole = _grouped(digits.substring(0, dot));
    final result = '$whole$_decimalMark${digits.substring(dot + 1)}';
    return negative && result != '0${_decimalMark}00' ? '-$result' : result;
  }

  String integer(int value) {
    final s = value.abs().toString();
    return '${value < 0 ? '-' : ''}${_grouped(s)}';
  }

  /// UTC anı iş saat diliminde gösterir: `21.11.2026 14:05` (tr) / `2026-11-21 14:05` (en).
  String dateTime(DateTime utc) {
    final t = _local(utc);
    return '${date(t)} ${_two(t.hour)}:${_two(t.minute)}';
  }

  String time(DateTime utc) {
    final t = _local(utc);
    return '${_two(t.hour)}:${_two(t.minute)}';
  }

  /// Takvim günü (saat dilimi dönüşümü yok).
  String date(DateTime d) =>
      _tr ? '${_two(d.day)}.${_two(d.month)}.${d.year}' : '${d.year}-${_two(d.month)}-${_two(d.day)}';

  static DateTime _local(DateTime utc) => tz.TZDateTime.from(utc.toUtc(), tz.getLocation(businessTimeZone));

  static String _two(int v) => v.toString().padLeft(2, '0');

  String _grouped(String digits) {
    final b = StringBuffer();
    for (var i = 0; i < digits.length; i++) {
      if (i > 0 && (digits.length - i) % 3 == 0) b.write(_group);
      b.write(digits[i]);
    }
    return b.toString();
  }
}
