import 'package:flutter/services.dart';
import 'package:material_ui/material_ui.dart';

import 'sgt_tokens.dart';

/// Ekranların palete erişimi: `context.palette`.
class SgtColors extends ThemeExtension<SgtColors> {
  const SgtColors(this.palette);

  final SgtPalette palette;

  @override
  SgtColors copyWith({SgtPalette? palette}) => SgtColors(palette ?? this.palette);

  @override
  SgtColors lerp(SgtColors? other, double t) => t < 0.5 ? this : (other ?? this);
}

extension SgtThemeContext on BuildContext {
  SgtPalette get palette => Theme.of(this).extension<SgtColors>()!.palette;
}

/// Token'lardan ThemeData. Bileşen biçimleri burada bir kez kurulur; ekranlar renk/yarıçap yazmaz.
abstract final class SgtTheme {
  static ThemeData light() => _build(SgtTokens.light, Brightness.light);

  static ThemeData dark() => _build(SgtTokens.dark, Brightness.dark);

  static const _shape = RoundedRectangleBorder(borderRadius: BorderRadius.all(Radius.circular(SgtTokens.radius)));

  static ThemeData _build(SgtPalette p, Brightness brightness) {
    final scheme = ColorScheme(
      brightness: brightness,
      primary: p.primary,
      onPrimary: p.onPrimary,
      secondary: p.primary,
      onSecondary: p.onPrimary,
      error: p.negative,
      onError: p.onPrimary,
      surface: p.surface,
      onSurface: p.text,
      onSurfaceVariant: p.textMuted,
      outline: p.borderStrong,
      outlineVariant: p.border,
      surfaceContainerLowest: p.surface,
      surfaceContainerLow: p.surface,
      surfaceContainer: p.surfaceAlt,
      surfaceContainerHigh: p.surfaceAlt,
      surfaceContainerHighest: p.surfaceAlt,
    );
    final text = _textTheme(p);
    final border = OutlineInputBorder(
      borderRadius: const BorderRadius.all(Radius.circular(SgtTokens.radius)),
      borderSide: BorderSide(color: p.borderStrong),
    );

    return ThemeData(
      useMaterial3: true,
      brightness: brightness,
      colorScheme: scheme,
      fontFamily: SgtTokens.fontFamily,
      scaffoldBackgroundColor: p.background,
      canvasColor: p.surface,
      dividerColor: p.border,
      textTheme: text,
      visualDensity: VisualDensity.compact,
      materialTapTargetSize: MaterialTapTargetSize.padded,
      splashFactory: NoSplash.splashFactory, // süs animasyonu yok (9a)
      pageTransitionsTheme: const PageTransitionsTheme(
        builders: {
          TargetPlatform.android: _NoTransitionBuilder(),
          TargetPlatform.iOS: _NoTransitionBuilder(),
        },
      ),
      extensions: [SgtColors(p)],
      appBarTheme: AppBarTheme(
        backgroundColor: p.header,
        foregroundColor: p.onHeader,
        elevation: 0,
        scrolledUnderElevation: 0,
        toolbarHeight: SgtTokens.headerHeight,
        centerTitle: false,
        titleSpacing: SgtTokens.space2,
        systemOverlayStyle: SystemUiOverlayStyle.light,
        titleTextStyle: TextStyle(
          fontFamily: SgtTokens.fontFamily,
          fontSize: SgtTokens.fontTitle,
          fontWeight: FontWeight.w600,
          color: p.onHeader,
        ),
        iconTheme: IconThemeData(color: p.onHeader, size: SgtTokens.iconSize),
      ),
      iconTheme: IconThemeData(color: p.textMuted, size: SgtTokens.iconSize),
      dividerTheme: DividerThemeData(color: p.border, thickness: SgtTokens.borderWidth, space: SgtTokens.borderWidth),
      cardTheme: CardThemeData(
        color: p.surface,
        elevation: 0,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: const BorderRadius.all(Radius.circular(SgtTokens.radius)),
          side: BorderSide(color: p.border),
        ),
      ),
      listTileTheme: ListTileThemeData(
        dense: true,
        minTileHeight: SgtTokens.rowHeight,
        contentPadding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3),
        iconColor: p.textMuted,
        textColor: p.text,
        shape: const RoundedRectangleBorder(),
      ),
      inputDecorationTheme: InputDecorationTheme(
        isDense: true,
        filled: true,
        fillColor: p.surface,
        contentPadding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3, vertical: SgtTokens.space3),
        border: border,
        enabledBorder: border,
        focusedBorder: border.copyWith(borderSide: BorderSide(color: p.focus, width: 2)),
        errorBorder: border.copyWith(borderSide: BorderSide(color: p.negative)),
        labelStyle: TextStyle(fontFamily: SgtTokens.fontFamily, color: p.textMuted, fontSize: SgtTokens.fontBody),
        hintStyle: TextStyle(fontFamily: SgtTokens.fontFamily, color: p.textMuted, fontSize: SgtTokens.fontBody),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: p.primary,
          foregroundColor: p.onPrimary,
          minimumSize: const Size(0, SgtTokens.controlHeight),
          padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space4),
          shape: _shape,
          textStyle: const TextStyle(fontFamily: SgtTokens.fontFamily, fontSize: SgtTokens.fontBody, fontWeight: FontWeight.w600),
          splashFactory: NoSplash.splashFactory,
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: p.text,
          minimumSize: const Size(0, SgtTokens.controlHeight),
          padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space4),
          side: BorderSide(color: p.borderStrong),
          shape: _shape,
          textStyle: const TextStyle(fontFamily: SgtTokens.fontFamily, fontSize: SgtTokens.fontBody, fontWeight: FontWeight.w500),
          splashFactory: NoSplash.splashFactory,
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: p.primary,
          shape: _shape,
          splashFactory: NoSplash.splashFactory,
        ),
      ),
      iconButtonTheme: IconButtonThemeData(style: IconButton.styleFrom(shape: _shape)),
      drawerTheme: DrawerThemeData(
        backgroundColor: p.surface,
        shape: const RoundedRectangleBorder(),
        endShape: const RoundedRectangleBorder(),
        width: 300,
      ),
      snackBarTheme: SnackBarThemeData(
        behavior: SnackBarBehavior.fixed,
        backgroundColor: p.header,
        contentTextStyle: TextStyle(fontFamily: SgtTokens.fontFamily, color: p.onHeader, fontSize: SgtTokens.fontBody),
        shape: const RoundedRectangleBorder(),
      ),
      dialogTheme: DialogThemeData(backgroundColor: p.surface, shape: _shape),
      progressIndicatorTheme: ProgressIndicatorThemeData(color: p.primary, linearTrackColor: p.border),
      tabBarTheme: TabBarThemeData(
        labelColor: p.text,
        unselectedLabelColor: p.textMuted,
        indicatorColor: p.primary,
        dividerColor: p.border,
        indicatorSize: TabBarIndicatorSize.tab,
        labelStyle: const TextStyle(fontFamily: SgtTokens.fontFamily, fontSize: SgtTokens.fontBody, fontWeight: FontWeight.w600),
        unselectedLabelStyle: const TextStyle(fontFamily: SgtTokens.fontFamily, fontSize: SgtTokens.fontBody),
      ),
    );
  }

  static TextTheme _textTheme(SgtPalette p) {
    const tabular = [FontFeature.tabularFigures()];
    TextStyle s(double size, FontWeight w, Color c) =>
        TextStyle(fontFamily: SgtTokens.fontFamily, fontSize: size, fontWeight: w, color: c, height: 1.3, fontFeatures: tabular);
    return TextTheme(
      headlineSmall: s(SgtTokens.fontFigure, FontWeight.w600, p.text),
      titleLarge: s(SgtTokens.fontTitle, FontWeight.w600, p.text),
      titleMedium: s(SgtTokens.fontBodyLarge, FontWeight.w600, p.text),
      titleSmall: s(SgtTokens.fontBody, FontWeight.w600, p.text),
      bodyLarge: s(SgtTokens.fontBodyLarge, FontWeight.w400, p.text),
      bodyMedium: s(SgtTokens.fontBody, FontWeight.w400, p.text),
      bodySmall: s(SgtTokens.fontCaption, FontWeight.w400, p.textMuted),
      labelLarge: s(SgtTokens.fontBody, FontWeight.w600, p.text),
      labelMedium: s(SgtTokens.fontCaption, FontWeight.w600, p.textMuted),
      labelSmall: s(SgtTokens.fontCaption, FontWeight.w500, p.textMuted),
    );
  }
}

/// Sayfa geçişinde animasyon yok (9a: süs animasyonu kullanılmaz).
class _NoTransitionBuilder extends PageTransitionsBuilder {
  const _NoTransitionBuilder();

  @override
  Widget buildTransitions<T>(
    PageRoute<T> route,
    BuildContext context,
    Animation<double> animation,
    Animation<double> secondaryAnimation,
    Widget child,
  ) =>
      child;
}
