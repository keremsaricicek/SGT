import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:go_router/go_router.dart';
import 'package:material_ui/material_ui.dart';

import '../../access/user_access.dart';
import '../../app/mobile_screens.dart';
import '../../app/services.dart';
import '../../shared/load_state.dart';
import '../../theme/sgt_theme.dart';
import '../../theme/sgt_tokens.dart';
import '../../ui/sgt_widgets.dart';

/// Ana menü (Y2, KK3): yalnızca yetkili ve mobilde karşılığı olan ekranlar, alanlara göre gruplu.
class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final services = AppServices.of(context);
    final l = context.l10n;
    return SgtPage(
      title: l.appTitle,
      subtitle: l.appSubtitle,
      actions: [
        IconButton(
          tooltip: l.refresh,
          icon: const Icon(FluentIcons.arrow_clockwise_24_regular),
          onPressed: services.access.load,
        ),
        IconButton(
          tooltip: l.signOut,
          icon: const Icon(FluentIcons.sign_out_24_regular),
          onPressed: services.auth.signOut,
        ),
      ],
      body: ListenableBuilder(
        listenable: services.access,
        builder: (context, _) => switch (services.access.state) {
          Idle() || Loading() => const LoadingPanel(),
          Failed(:final error) => FailurePanel(error, onRetry: services.access.load),
          Loaded(:final value) => _Menu(value),
        },
      ),
    );
  }
}

class _Menu extends StatelessWidget {
  const _Menu(this.access);

  final UserAccess access;

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    final visible = MobileScreen.all.where((s) => access.canView(s.code)).toList();
    if (visible.isEmpty) {
      return MessagePanel(icon: FluentIcons.lock_closed_24_regular, text: l.menuEmpty);
    }
    return ListView(
      children: [
        for (final area in MenuArea.values)
          if (visible.any((s) => s.area == area)) ...[
            SectionHeader(MobileScreen.areaTitle(l, area)),
            for (final s in visible.where((s) => s.area == area)) _MenuRow(s),
          ],
      ],
    );
  }
}

class _MenuRow extends StatelessWidget {
  const _MenuRow(this.screen);

  final MobileScreen screen;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    return Material(
      color: p.surface,
      child: InkWell(
        onTap: () => context.push(screen.route),
        child: Container(
          height: 48,
          padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3),
          decoration: BoxDecoration(border: Border(bottom: BorderSide(color: p.border))),
          child: Row(
            children: [
              Icon(screen.icon, color: p.primary, size: SgtTokens.iconSize),
              const SizedBox(width: SgtTokens.space3),
              Expanded(child: Text(screen.title(context.l10n), style: t.bodyLarge)),
              Text(screen.code, style: t.bodySmall),
              const SizedBox(width: SgtTokens.space2),
              Icon(FluentIcons.chevron_right_20_regular, size: 16, color: p.textMuted),
            ],
          ),
        ),
      ),
    );
  }
}
