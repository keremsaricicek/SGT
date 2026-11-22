import 'package:decimal/decimal.dart';
import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:material_ui/material_ui.dart';

import '../api/api_exception.dart';
import '../app/services.dart';
import '../l10n/gen/app_localizations.dart';
import '../shared/formatting.dart';
import '../theme/sgt_theme.dart';
import '../theme/sgt_tokens.dart';

extension SgtContext on BuildContext {
  AppLocalizations get l10n => AppLocalizations.of(this);

  SgtFormat get fmt => SgtFormat(Localizations.localeOf(this).languageCode);
}

/// Standart ekran çerçevesi: başlık çubuğu, gövde, isteğe bağlı alt eylem çubuğu, durum çubuğu.
class SgtPage extends StatelessWidget {
  const SgtPage({super.key, required this.title, this.subtitle, this.actions = const [], required this.body, this.bottomAction});

  final String title;
  final String? subtitle;
  final List<Widget> actions;
  final Widget body;
  final Widget? bottomAction;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    return Scaffold(
      appBar: AppBar(
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(title),
            if (subtitle != null)
              Text(subtitle!, style: TextStyle(fontSize: SgtTokens.fontCaption, color: p.onHeader.withValues(alpha: 0.7))),
          ],
        ),
        actions: actions,
      ),
      body: body,
      bottomNavigationBar: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (bottomAction != null)
            DecoratedBox(
              decoration: BoxDecoration(color: p.surface, border: Border(top: BorderSide(color: p.border))),
              child: Padding(padding: const EdgeInsets.all(SgtTokens.space3), child: bottomAction),
            ),
          const StatusStrip(),
        ],
      ),
    );
  }
}

/// Alt durum çubuğu (klasik iş uygulaması): kullanıcı ve terminal.
class StatusStrip extends StatelessWidget {
  const StatusStrip({super.key});

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final services = AppServices.of(context);
    final user = services.auth.session?.username ?? '';
    final style = TextStyle(fontSize: SgtTokens.fontCaption, color: p.onHeader.withValues(alpha: 0.8));
    return Container(
      color: p.header,
      child: SafeArea(
        top: false,
        child: SizedBox(
          height: SgtTokens.statusBarHeight,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3),
            child: Row(
              children: [
                Icon(FluentIcons.person_16_regular, size: 12, color: style.color),
                const SizedBox(width: SgtTokens.space1),
                Text(user, style: style),
                const Spacer(),
                Text(context.l10n.statusTerminal(services.terminalId), style: style),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

/// Bölüm başlığı: küçük büyük harfli etiket, ince alt çizgi.
class SectionHeader extends StatelessWidget {
  const SectionHeader(this.title, {super.key, this.trailing});

  final String title;
  final String? trailing;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final style = Theme.of(context).textTheme.labelMedium!.copyWith(letterSpacing: 0.6);
    return Container(
      decoration: BoxDecoration(color: p.surfaceAlt, border: Border(top: BorderSide(color: p.border), bottom: BorderSide(color: p.border))),
      padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3, vertical: 6),
      child: Row(
        children: [
          Expanded(child: Text(title.toUpperCase(), style: style)),
          if (trailing != null) Text(trailing!, style: style.copyWith(letterSpacing: 0, fontWeight: FontWeight.w500)),
        ],
      ),
    );
  }
}

enum TagTone { neutral, positive, negative, warning }

/// Durum etiketi: dikdörtgen, ince çerçeve (hap biçimi değil).
class StatusTag extends StatelessWidget {
  const StatusTag(this.text, {super.key, this.tone = TagTone.neutral});

  final String text;
  final TagTone tone;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final color = switch (tone) {
      TagTone.neutral => p.textMuted,
      TagTone.positive => p.positive,
      TagTone.negative => p.negative,
      TagTone.warning => p.warning,
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 1),
      decoration: BoxDecoration(
        border: Border.all(color: color),
        borderRadius: const BorderRadius.all(Radius.circular(SgtTokens.radius)),
      ),
      child: Text(text, style: TextStyle(fontSize: 10, fontWeight: FontWeight.w600, color: color, letterSpacing: 0.4)),
    );
  }
}

/// Etiket – değer satırı.
class KeyValueRow extends StatelessWidget {
  const KeyValueRow(this.label, this.value, {super.key, this.valueColor, this.strong = false});

  final String label;
  final String value;
  final Color? valueColor;
  final bool strong;

  @override
  Widget build(BuildContext context) {
    final t = Theme.of(context).textTheme;
    return Container(
      constraints: const BoxConstraints(minHeight: 36),
      padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3, vertical: 6),
      decoration: BoxDecoration(border: Border(bottom: BorderSide(color: context.palette.border))),
      child: Row(
        children: [
          Expanded(child: Text(label, style: t.bodyMedium!.copyWith(color: context.palette.textMuted))),
          Text(value, style: (strong ? t.titleMedium : t.bodyMedium)!.copyWith(color: valueColor)),
        ],
      ),
    );
  }
}

/// Yoğun tablo hücresi tanımı.
class GridCell {
  const GridCell(this.text, {this.flex = 1, this.alignEnd = false, this.color, this.strong = false, this.muted = false});

  final String text;
  final int flex;
  final bool alignEnd;
  final Color? color;
  final bool strong;
  final bool muted;
}

/// Yoğun tablo satırı; [header] ise başlık biçimi.
class GridRow extends StatelessWidget {
  const GridRow(this.cells, {super.key, this.header = false, this.highlight = false});

  final List<GridCell> cells;
  final bool header;
  final bool highlight;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    final base = header ? t.labelMedium! : t.bodyMedium!;
    return Container(
      constraints: BoxConstraints(minHeight: header ? 28 : 36),
      padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3, vertical: 6),
      decoration: BoxDecoration(
        color: header || highlight ? p.surfaceAlt : p.surface,
        border: Border(bottom: BorderSide(color: p.border)),
      ),
      child: Row(
        children: [
          for (var i = 0; i < cells.length; i++) ...[
            if (i > 0) const SizedBox(width: SgtTokens.space2),
            Expanded(
              flex: cells[i].flex,
              child: Text(
                cells[i].text,
                textAlign: cells[i].alignEnd ? TextAlign.end : TextAlign.start,
                overflow: TextOverflow.ellipsis,
                style: base.copyWith(
                  color: cells[i].color ?? (cells[i].muted ? p.textMuted : null),
                  fontWeight: cells[i].strong || highlight ? FontWeight.w600 : null,
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

/// Boş durum, uyarı veya hata mesajı paneli.
class MessagePanel extends StatelessWidget {
  const MessagePanel({super.key, required this.icon, required this.text, this.tone = TagTone.neutral, this.action});

  final IconData icon;
  final String text;
  final TagTone tone;
  final Widget? action;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final color = switch (tone) {
      TagTone.neutral => p.textMuted,
      TagTone.positive => p.positive,
      TagTone.negative => p.negative,
      TagTone.warning => p.warning,
    };
    return Padding(
      padding: const EdgeInsets.all(SgtTokens.space3),
      child: Container(
        padding: const EdgeInsets.all(SgtTokens.space3),
        decoration: BoxDecoration(
          color: p.surface,
          border: Border(left: BorderSide(color: color, width: 3), top: BorderSide(color: p.border), right: BorderSide(color: p.border), bottom: BorderSide(color: p.border)),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(icon, size: SgtTokens.iconSize, color: color),
            const SizedBox(width: SgtTokens.space2),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Padding(padding: const EdgeInsets.only(top: 2), child: Text(text, style: Theme.of(context).textTheme.bodyMedium)),
                  if (action != null) ...[const SizedBox(height: SgtTokens.space2), action!],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// API hatasını kullanıcı metnine çevirir (sunucu başlığı varsa onu gösterir, KK5).
String failureText(BuildContext context, ApiException e) {
  final l = context.l10n;
  return switch (e.kind) {
    ApiFailure.network => l.errNetwork,
    ApiFailure.unauthorized => l.errSession,
    ApiFailure.forbidden => l.errForbidden,
    ApiFailure.notFound => e.title ?? l.errNotFound,
    ApiFailure.conflict => e.title ?? l.errConflict,
    ApiFailure.validation => e.title ?? l.errValidation,
    ApiFailure.server => l.errServer,
    ApiFailure.invalidResponse => l.errInvalidResponse,
  };
}

class FailurePanel extends StatelessWidget {
  const FailurePanel(this.error, {super.key, this.onRetry});

  final ApiException error;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => MessagePanel(
        icon: error.kind == ApiFailure.network ? FluentIcons.wifi_off_24_regular : FluentIcons.error_circle_24_regular,
        text: failureText(context, error),
        tone: TagTone.negative,
        action: onRetry == null ? null : OutlinedButton(onPressed: onRetry, child: Text(context.l10n.retry)),
      );
}

class LoadingPanel extends StatelessWidget {
  const LoadingPanel({super.key});

  @override
  Widget build(BuildContext context) => const Align(alignment: Alignment.topCenter, child: LinearProgressIndicator(minHeight: 2));
}

/// Pozitif/negatif tutar rengi (casino sonucu, kazanç).
Color? signedColor(BuildContext context, Decimal value) =>
    value < Decimal.zero ? context.palette.negative : null;
