import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:material_ui/material_ui.dart';

import '../../../access/screen_codes.dart';
import '../../../app/services.dart';
import '../../../shared/load_state.dart';
import '../../../ui/sgt_widgets.dart';
import 'floor_view_models.dart';

/// Masa haritası / pit görünümü (C1, KK11). Salt okunur. Masa konum bilgisi API'de olmadığı için liste.
class TablesScreen extends StatefulWidget {
  const TablesScreen({super.key});

  @override
  State<TablesScreen> createState() => _TablesScreenState();
}

class _TablesScreenState extends State<TablesScreen> {
  TablesViewModel? _vm;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_vm == null) {
      final s = AppServices.of(context);
      _vm = TablesViewModel(s.floor, canSeeRatings: s.access.access.canView(ScreenCodes.liveRating))..load();
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
    final f = context.fmt;
    return SgtPage(
      title: l.screenTables,
      subtitle: l.readOnly,
      actions: [IconButton(tooltip: l.refresh, icon: const Icon(FluentIcons.arrow_clockwise_24_regular), onPressed: vm.load)],
      body: ListenableBuilder(
        listenable: vm,
        builder: (context, _) => switch (vm.overview) {
          Idle() || Loading() => const LoadingPanel(),
          Failed(:final error) => FailurePanel(error, onRetry: vm.load),
          Loaded(:final value) => ListView(
              children: [
                SectionHeader(l.areaLive, trailing: l.tablesSummary(value.openCount, value.rows.length)),
                GridRow(header: true, [
                  GridCell(l.colTable, flex: 3),
                  GridCell(l.colGame, flex: 4),
                  GridCell(l.limits, flex: 6),
                  if (vm.canSeeRatings) GridCell(l.colRatings, flex: 2, alignEnd: true),
                  GridCell(l.colStatus, flex: 3, alignEnd: true),
                ]),
                for (final r in value.rows)
                  _TableRow(r, showRatings: vm.canSeeRatings, limits: '${f.limit(r.table.minBet)}–${f.limit(r.table.maxBet)} ${r.table.currency}'),
              ],
            ),
        },
      ),
    );
  }
}

class _TableRow extends StatelessWidget {
  const _TableRow(this.row, {required this.showRatings, required this.limits});

  final FloorTableRow row;
  final bool showRatings;
  final String limits;

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    final t = row.table;
    return Stack(
      children: [
        GridRow([
          GridCell(t.code, flex: 3, strong: true),
          GridCell(t.gameType, flex: 4, muted: !t.isOpen),
          GridCell(limits, flex: 6, muted: !t.isOpen),
          if (showRatings) GridCell(t.isOpen ? '${row.runningRatings}' : '–', flex: 2, alignEnd: true),
          const GridCell('', flex: 3),
        ]),
        Positioned(
          right: 12,
          top: 0,
          bottom: 0,
          child: Center(
            child: StatusTag(t.isOpen ? l.tableOpen : l.tableClosed, tone: t.isOpen ? TagTone.positive : TagTone.neutral),
          ),
        ),
      ],
    );
  }
}
