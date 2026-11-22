import 'package:material_ui/material_ui.dart';

import '../../api/generated/sgt_api.g.dart';
import '../../theme/sgt_theme.dart';
import '../../theme/sgt_tokens.dart';
import '../../ui/sgt_widgets.dart';

/// Seçilen oyuncunun kimlik satırı. Aramadan gelen özet yoksa (doğrudan bağlantı) yalnızca başlık gösterilir.
class PlayerHeader extends StatelessWidget {
  const PlayerHeader(this.player, {super.key});

  final PlayerSummary? player;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    final l = context.l10n;
    final pl = player;
    return Container(
      color: p.surface,
      padding: const EdgeInsets.fromLTRB(SgtTokens.space3, SgtTokens.space3, SgtTokens.space3, SgtTokens.space3),
      child: pl == null
          ? Text(l.screenPlayer, style: t.titleMedium)
          : Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('${pl.firstName} ${pl.lastName}', style: t.titleMedium),
                      const SizedBox(height: 2),
                      Text('${pl.playerNumber}  ·  ${pl.activeCardNumber ?? l.noCard}', style: t.bodySmall),
                    ],
                  ),
                ),
                if (pl.isRestricted) StatusTag(l.restricted, tone: TagTone.negative),
              ],
            ),
    );
  }
}
