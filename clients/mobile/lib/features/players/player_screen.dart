import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:go_router/go_router.dart';
import 'package:material_ui/material_ui.dart';

import '../../api/generated/sgt_api.g.dart';
import '../../app/services.dart';
import '../../shared/load_state.dart';
import '../../theme/sgt_theme.dart';
import '../../theme/sgt_tokens.dart';
import '../../ui/sgt_widgets.dart';
import 'player_header.dart';
import 'player_view_model.dart';

/// Oyuncu: şu an nerede (O2) ve puan (L1). Bölümler yetkiye göre görünür.
class PlayerScreen extends StatefulWidget {
  const PlayerScreen({super.key, required this.playerId, this.summary});

  final String playerId;
  final PlayerSummary? summary;

  @override
  State<PlayerScreen> createState() => _PlayerScreenState();
}

class _PlayerScreenState extends State<PlayerScreen> {
  PlayerViewModel? _vm;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_vm == null) {
      final s = AppServices.of(context);
      _vm = PlayerViewModel(playerId: widget.playerId, players: s.players, loyalty: s.loyalty, access: s.access.access)..load();
    }
  }

  @override
  void dispose() {
    _vm?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final vm = _vm!;
    final l = context.l10n;
    return SgtPage(
      title: l.screenPlayer,
      actions: [IconButton(tooltip: l.refresh, icon: const Icon(FluentIcons.arrow_clockwise_24_regular), onPressed: vm.load)],
      bottomAction: vm.canRequestComp
          ? FilledButton.icon(
              onPressed: () => context.push('/players/${widget.playerId}/comp', extra: widget.summary),
              icon: const Icon(FluentIcons.gift_24_regular, size: 18),
              label: Text(l.requestComp),
            )
          : null,
      body: ListenableBuilder(
        listenable: vm,
        builder: (context, _) => ListView(
          children: [
            PlayerHeader(widget.summary),
            if (vm.showLocation) ...[
              SectionHeader(l.sectionLocation),
              _Location(vm.location, onRetry: vm.load),
            ],
            if (vm.showLoyalty) ...[
              SectionHeader(l.sectionLoyalty),
              _Loyalty(vm.loyalty, onRetry: vm.load),
            ],
          ],
        ),
      ),
    );
  }
}

class _Location extends StatelessWidget {
  const _Location(this.state, {required this.onRetry});

  final LoadState<PlayerLocationDto?> state;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    return switch (state) {
      Idle() || Loading() => const LoadingPanel(),
      Failed(:final error) => FailurePanel(error, onRetry: onRetry),
      Loaded(value: null) => MessagePanel(icon: FluentIcons.location_24_regular, text: l.notPlaying),
      Loaded(:final value?) => Container(
          color: p.surface,
          padding: const EdgeInsets.all(SgtTokens.space3),
          child: Row(
            children: [
              Container(
                width: 56,
                height: 40,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  color: p.primary,
                  borderRadius: const BorderRadius.all(Radius.circular(SgtTokens.radius)),
                ),
                child: Text(value.tableCode, style: TextStyle(color: p.onPrimary, fontWeight: FontWeight.w600, fontSize: SgtTokens.fontBody)),
              ),
              const SizedBox(width: SgtTokens.space3),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(l.playingAt(value.tableCode), style: t.bodyLarge),
                    Text('${l.since}: ${context.fmt.dateTime(value.since)}', style: t.bodySmall),
                  ],
                ),
              ),
            ],
          ),
        ),
    };
  }
}

class _Loyalty extends StatelessWidget {
  const _Loyalty(this.state, {required this.onRetry});

  final LoadState<LoyaltySummary> state;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    final f = context.fmt;
    final p = context.palette;
    return switch (state) {
      Idle() || Loading() => const LoadingPanel(),
      Failed(:final error) => FailurePanel(error, onRetry: onRetry),
      Loaded(:final value) => Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            ColoredBox(
              color: p.surface,
              child: Column(
                children: [
                  KeyValueRow(l.balance, f.integer(value.balance), strong: true),
                  KeyValueRow(l.tier, value.tier),
                  KeyValueRow(l.lifetimeEarned, f.integer(value.lifetimeEarned)),
                ],
              ),
            ),
            SectionHeader(l.recentEntries),
            if (value.recentEntries.isEmpty)
              MessagePanel(icon: FluentIcons.history_24_regular, text: l.noEntries)
            else ...[
              GridRow(header: true, [GridCell(l.date, flex: 4), GridCell(l.colKind, flex: 3), GridCell(l.fieldPoints, flex: 3, alignEnd: true)]),
              for (final e in value.recentEntries)
                GridRow([
                  GridCell(f.dateTime(e.at), flex: 4, muted: true),
                  GridCell(_kind(context, e.kind), flex: 3),
                  GridCell(
                    '${e.points > 0 ? '+' : ''}${f.integer(e.points)}',
                    flex: 3,
                    alignEnd: true,
                    strong: true,
                    color: e.points < 0 ? p.negative : p.positive,
                  ),
                ]),
            ],
          ],
        ),
    };
  }

  static String _kind(BuildContext context, String kind) {
    final l = context.l10n;
    return switch (kind) {
      'Earned' => l.kindEarned,
      'Redeemed' => l.kindRedeemed,
      'Adjusted' => l.kindAdjusted,
      _ => kind,
    };
  }
}
