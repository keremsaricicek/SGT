import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:go_router/go_router.dart';
import 'package:material_ui/material_ui.dart';

import '../../../api/generated/sgt_api.g.dart';
import '../../../app/services.dart';
import '../../../l10n/gen/app_localizations.dart';
import '../../../shared/load_state.dart';
import '../../../theme/sgt_theme.dart';
import '../../../theme/sgt_tokens.dart';
import '../../../ui/sgt_widgets.dart';
import 'player_search_view_model.dart';

/// Aramanın hangi menüden açıldığı: başlığı ve seçimden sonra gidilecek ekranı belirler.
enum SearchPurpose {
  player('/players'),
  locator('/locator'),
  loyalty('/loyalty'),
  comp('/comp');

  const SearchPurpose(this.route);

  final String route;

  String title(AppLocalizations l) => switch (this) {
        player => l.screenPlayerSearch,
        locator => l.screenPlayerLocator,
        loyalty => l.screenLoyaltyPoints,
        comp => l.screenCompRequest,
      };

  String target(String playerId) => this == comp ? '/players/$playerId/comp' : '/players/$playerId';
}

/// Oyuncu arama (O1, KK6).
class PlayerSearchScreen extends StatefulWidget {
  const PlayerSearchScreen({super.key, required this.purpose, this.initialQuery});

  final SearchPurpose purpose;
  final String? initialQuery;

  @override
  State<PlayerSearchScreen> createState() => _PlayerSearchScreenState();
}

class _PlayerSearchScreenState extends State<PlayerSearchScreen> {
  late final PlayerSearchViewModel _vm;
  late final TextEditingController _query;

  @override
  void initState() {
    super.initState();
    _query = TextEditingController(text: widget.initialQuery);
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_initialized) {
      _initialized = true;
      _vm = PlayerSearchViewModel(AppServices.of(context).players);
      if (widget.initialQuery != null) _vm.search(widget.initialQuery!);
    }
  }

  bool _initialized = false;

  @override
  void dispose() {
    _vm.dispose();
    _query.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    final p = context.palette;
    return SgtPage(
      title: widget.purpose.title(l),
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Container(
            color: p.surface,
            padding: const EdgeInsets.all(SgtTokens.space3),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _query,
                    autofocus: widget.initialQuery == null,
                    textInputAction: TextInputAction.search,
                    autocorrect: false,
                    enableSuggestions: false, // klavye öğrenmesine oyuncu adı gitmez
                    decoration: InputDecoration(
                      hintText: l.searchHint,
                      prefixIcon: const Icon(FluentIcons.search_24_regular, size: 18),
                    ),
                    onSubmitted: _vm.search,
                  ),
                ),
                const SizedBox(width: SgtTokens.space2),
                FilledButton(onPressed: () => _vm.search(_query.text), child: const Icon(FluentIcons.search_24_regular, size: 18)),
              ],
            ),
          ),
          Expanded(
            child: ListenableBuilder(
              listenable: _vm,
              builder: (context, _) => switch (_vm.results) {
                Idle() => _vm.tooShort
                    ? MessagePanel(icon: FluentIcons.info_24_regular, text: l.searchMinChars)
                    : const SizedBox.shrink(),
                Loading() => const LoadingPanel(),
                Failed(:final error) => FailurePanel(error, onRetry: () => _vm.search(_query.text)),
                Loaded(:final value) when value.isEmpty =>
                  MessagePanel(icon: FluentIcons.info_24_regular, text: l.searchNoResults),
                Loaded(:final value) => _Results(value, widget.purpose),
              },
            ),
          ),
        ],
      ),
    );
  }
}

class _Results extends StatelessWidget {
  const _Results(this.players, this.purpose);

  final List<PlayerSummary> players;
  final SearchPurpose purpose;

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    return ListView(
      children: [
        SectionHeader(l.areaPlayer, trailing: l.resultCount(players.length)),
        for (final pl in players) _PlayerRow(pl, onTap: () => context.push(purpose.target(pl.playerId), extra: pl)),
      ],
    );
  }
}

class _PlayerRow extends StatelessWidget {
  const _PlayerRow(this.player, {required this.onTap});

  final PlayerSummary player;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    final l = context.l10n;
    return Material(
      color: p.surface,
      child: InkWell(
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3, vertical: SgtTokens.space2),
          decoration: BoxDecoration(border: Border(bottom: BorderSide(color: p.border))),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Text(player.playerNumber, style: t.titleSmall),
                        const SizedBox(width: SgtTokens.space2),
                        Expanded(child: Text('${player.firstName} ${player.lastName}', style: t.bodyMedium, overflow: TextOverflow.ellipsis)),
                      ],
                    ),
                    const SizedBox(height: 2),
                    Text(player.activeCardNumber ?? l.noCard, style: t.bodySmall),
                  ],
                ),
              ),
              if (player.isRestricted) ...[StatusTag(l.restricted, tone: TagTone.negative), const SizedBox(width: SgtTokens.space2)],
              Icon(FluentIcons.chevron_right_20_regular, size: 16, color: p.textMuted),
            ],
          ),
        ),
      ),
    );
  }
}
