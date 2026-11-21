// SGT tasarım token'ları — mobilin TÜM tema değerleri yalnızca bu dosyadadır.
//
// `design/tokens/sgt-tokens.json` (style-dictionary, `design/tokens/build/` Dart çıktısı) geldiğinde bu dosya
// üretilen dosyayla değiştirilir; aynı sınıf ve alan adları korunursa başka dosyaya dokunmak gerekmez.
// Kurallar (kararlar 9a): köşe yarıçapı ≤ 3 px, hap düğme yok, büyük yuvarlak kart yok, süs animasyonu yok.
import 'dart:ui' show Color;

/// Bir temanın renkleri.
class SgtPalette {
  const SgtPalette({
    required this.background,
    required this.surface,
    required this.surfaceAlt,
    required this.border,
    required this.borderStrong,
    required this.text,
    required this.textMuted,
    required this.primary,
    required this.onPrimary,
    required this.header,
    required this.onHeader,
    required this.positive,
    required this.negative,
    required this.warning,
    required this.focus,
  });

  final Color background;
  final Color surface;
  final Color surfaceAlt;
  final Color border;
  final Color borderStrong;
  final Color text;
  final Color textMuted;
  final Color primary;
  final Color onPrimary;

  /// Uygulama başlık çubuğu ve durum çubuğu (klasik iş uygulaması çerçevesi).
  final Color header;
  final Color onHeader;
  final Color positive;
  final Color negative;
  final Color warning;
  final Color focus;
}

abstract final class SgtTokens {
  static const light = SgtPalette(
    background: Color(0xFFF3F4F6),
    surface: Color(0xFFFFFFFF),
    surfaceAlt: Color(0xFFF7F8FA),
    border: Color(0xFFD9DDE3),
    borderStrong: Color(0xFFB4BBC6),
    text: Color(0xFF161B24),
    textMuted: Color(0xFF5A6372),
    primary: Color(0xFF1D4F91),
    onPrimary: Color(0xFFFFFFFF),
    header: Color(0xFF1A2333),
    onHeader: Color(0xFFF2F4F7),
    positive: Color(0xFF17704A),
    negative: Color(0xFFB42318),
    warning: Color(0xFF9A6200),
    focus: Color(0xFF2F6FD0),
  );

  static const dark = SgtPalette(
    background: Color(0xFF0E1116),
    surface: Color(0xFF151A21),
    surfaceAlt: Color(0xFF1B212A),
    border: Color(0xFF29313C),
    borderStrong: Color(0xFF3A4452),
    text: Color(0xFFE5E8ED),
    textMuted: Color(0xFF98A1AE),
    primary: Color(0xFF6C9BE0),
    onPrimary: Color(0xFF0B1220),
    header: Color(0xFF0A0D11),
    onHeader: Color(0xFFE5E8ED),
    positive: Color(0xFF4CBB86),
    negative: Color(0xFFF0736A),
    warning: Color(0xFFE2A93B),
    focus: Color(0xFF8DB4F0),
  );

  // Biçim
  static const double radius = 2; // ≤ 3 px (9a)
  static const double borderWidth = 1;

  // Boşluk (4'ün katları)
  static const double space1 = 4;
  static const double space2 = 8;
  static const double space3 = 12;
  static const double space4 = 16;

  // Yoğunluk
  static const double rowHeight = 40;
  static const double controlHeight = 40;
  static const double headerHeight = 48;
  static const double statusBarHeight = 24;
  static const double iconSize = 20;

  // Tipografi
  static const String fontFamily = 'Inter';
  static const double fontCaption = 11;
  static const double fontBody = 13;
  static const double fontBodyLarge = 15;
  static const double fontTitle = 17;
  static const double fontFigure = 22;
}
