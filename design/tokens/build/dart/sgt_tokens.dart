// BU DOSYA ÜRETİLİR, ELLE DEĞİŞTİRMEYİN. Kaynak: design/tokens/sgt-tokens.json (npm run gen:tokens, Spec 013).
// ignore_for_file: public_member_api_docs
import 'dart:ui';

/// Tema renkleri; iki temada aynı alanlar bulunur.
class SgtPalette {
  const SgtPalette({required this.chrome, required this.canvas, required this.panel, required this.panelHead, required this.well, required this.rule, required this.ruleStrong, required this.rowAlt, required this.rowHover, required this.rowSelected, required this.ink, required this.inkSecondary, required this.inkMuted, required this.inkDisabled, required this.accent, required this.accentHover, required this.onAccent, required this.link, required this.focus, required this.moneyIn, required this.moneyOut, required this.warning, required this.warningBg, required this.danger, required this.dangerBg, required this.success, required this.successBg, required this.info, required this.infoBg, required this.restricted, required this.restrictedBg, required this.backdrop});
  final Color chrome;
  final Color canvas;
  final Color panel;
  final Color panelHead;
  final Color well;
  final Color rule;
  final Color ruleStrong;
  final Color rowAlt;
  final Color rowHover;
  final Color rowSelected;
  final Color ink;
  final Color inkSecondary;
  final Color inkMuted;
  final Color inkDisabled;
  final Color accent;
  final Color accentHover;
  final Color onAccent;
  final Color link;
  final Color focus;
  final Color moneyIn;
  final Color moneyOut;
  final Color warning;
  final Color warningBg;
  final Color danger;
  final Color dangerBg;
  final Color success;
  final Color successBg;
  final Color info;
  final Color infoBg;
  final Color restricted;
  final Color restrictedBg;
  final Color backdrop;

  static const SgtPalette dark = SgtPalette(
    chrome: Color(0xFF0C0F12),
    canvas: Color(0xFF121519),
    panel: Color(0xFF181C21),
    panelHead: Color(0xFF1E2329),
    well: Color(0xFF0E1114),
    rule: Color(0xFF2A3037),
    ruleStrong: Color(0xFF6B7581),
    rowAlt: Color(0xFF15191E),
    rowHover: Color(0xFF1F252C),
    rowSelected: Color(0xFF1B2C42),
    ink: Color(0xFFE3E6EA),
    inkSecondary: Color(0xFFB3BAC3),
    inkMuted: Color(0xFF929BA6),
    inkDisabled: Color(0xFF5C646E),
    accent: Color(0xFF2F6DBF),
    accentHover: Color(0xFF2A62AD),
    onAccent: Color(0xFFFFFFFF),
    link: Color(0xFF79AEF0),
    focus: Color(0xFF79AEF0),
    moneyIn: Color(0xFF4CC38A),
    moneyOut: Color(0xFFF1786C),
    warning: Color(0xFFE3B450),
    warningBg: Color(0xFF2E2617),
    danger: Color(0xFFF1786C),
    dangerBg: Color(0xFF371C1A),
    success: Color(0xFF4CC38A),
    successBg: Color(0xFF142B1E),
    info: Color(0xFF79AEF0),
    infoBg: Color(0xFF15263A),
    restricted: Color(0xFFC09AF2),
    restrictedBg: Color(0xFF2A2140),
    backdrop: Color(0xFF000000),
  );

  static const SgtPalette light = SgtPalette(
    chrome: Color(0xFFDFE3E8),
    canvas: Color(0xFFECEFF2),
    panel: Color(0xFFFFFFFF),
    panelHead: Color(0xFFF4F6F8),
    well: Color(0xFFFFFFFF),
    rule: Color(0xFFD3D8DE),
    ruleStrong: Color(0xFF7A8490),
    rowAlt: Color(0xFFF7F8FA),
    rowHover: Color(0xFFEAF0F7),
    rowSelected: Color(0xFFDBE7F6),
    ink: Color(0xFF171C22),
    inkSecondary: Color(0xFF3D4650),
    inkMuted: Color(0xFF58616C),
    inkDisabled: Color(0xFF9AA2AC),
    accent: Color(0xFF1F5FAE),
    accentHover: Color(0xFF1A5299),
    onAccent: Color(0xFFFFFFFF),
    link: Color(0xFF1A5299),
    focus: Color(0xFF1F5FAE),
    moneyIn: Color(0xFF157347),
    moneyOut: Color(0xFFB42318),
    warning: Color(0xFF8A5A00),
    warningBg: Color(0xFFFBF1D9),
    danger: Color(0xFFB42318),
    dangerBg: Color(0xFFFCEBE9),
    success: Color(0xFF157347),
    successBg: Color(0xFFE3F3EA),
    info: Color(0xFF1A5299),
    infoBg: Color(0xFFE4EDF8),
    restricted: Color(0xFF6D35B5),
    restrictedBg: Color(0xFFF0E8FB),
    backdrop: Color(0xFF0B0E12),
  );
}

/// Tema bağımsız ölçüler (mantıksal piksel) ve yazı aileleri.
abstract final class SgtMetrics {
  static const List<String> fontFamilyUi = ['IBM Plex Sans', 'Segoe UI', 'system-ui', 'sans-serif'];
  static const List<String> fontFamilyMono = ['IBM Plex Mono', 'Cascadia Mono', 'Consolas', 'monospace'];
  static const double fontSizeCaption = 11.0;
  static const double fontSizeSmall = 12.0;
  static const double fontSizeBody = 13.0;
  static const double fontSizeHeading = 15.0;
  static const double fontSizeTitle = 18.0;
  static const double fontSizeFigure = 22.0;
  static const double fontWeightRegular = 400.0;
  static const double fontWeightMedium = 500.0;
  static const double fontWeightSemibold = 600.0;
  static const double fontLineHeightTight = 1.20;
  static const double fontLineHeightBody = 1.45;
  static const double fontLetterSpacingCaption = 0.40;
  static const double space0 = 0.0;
  static const double space1 = 2.0;
  static const double space2 = 4.0;
  static const double space3 = 6.0;
  static const double space4 = 8.0;
  static const double space5 = 12.0;
  static const double space6 = 16.0;
  static const double space7 = 20.0;
  static const double space8 = 24.0;
  static const double space9 = 32.0;
  static const double radiusNone = 0.0;
  static const double radiusControl = 2.0;
  static const double radiusPanel = 2.0;
  static const double radiusDialog = 3.0;
  static const double borderWidth = 1.0;
  static const double borderMarker = 3.0;
  static const double borderFocus = 2.0;
  static const double controlHeightSmall = 24.0;
  static const double controlHeightMedium = 28.0;
  static const double controlHeightLarge = 32.0;
  static const double controlPaddingX = 10.0;
  static const double gridStandardRowHeight = 28.0;
  static const double gridStandardHeaderHeight = 28.0;
  static const double gridStandardCellPaddingX = 8.0;
  static const double gridStandardFontSize = 13.0;
  static const double gridCompactRowHeight = 24.0;
  static const double gridCompactHeaderHeight = 26.0;
  static const double gridCompactCellPaddingX = 6.0;
  static const double gridCompactFontSize = 12.0;
  static const double layoutTitleBarHeight = 36.0;
  static const double layoutStatusBarHeight = 24.0;
  static const double layoutSideNavWidth = 216.0;
  static const double layoutToolbarHeight = 44.0;
  static const double layoutContentMaxWidth = 1600.0;
}
