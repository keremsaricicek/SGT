import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:material_ui/material_ui.dart';

import '../../../app/services.dart';
import '../../../shared/load_state.dart';
import '../../../ui/sgt_widgets.dart';
import 'floor_view_models.dart';

/// Slot salonu / makine durumu (S1, KK12). Salt okunur; alana göre gruplu.
class SlotsScreen extends StatefulWidget {
  const SlotsScreen({super.key});

  @override
  State<SlotsScreen> createState() => _SlotsScreenState();
}

class _SlotsScreenState extends State<SlotsScreen> {
  SlotsViewModel? _vm;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _vm ??= SlotsViewModel(AppServices.of(context).floor)..load();
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
      title: l.screenSlots,
      subtitle: l.readOnly,
      actions: [IconButton(tooltip: l.refresh, icon: const Icon(FluentIcons.arrow_clockwise_24_regular), onPressed: vm.load)],
      body: ListenableBuilder(
        listenable: vm,
        builder: (context, _) => switch (vm.areas) {
          Idle() || Loading() => const LoadingPanel(),
          Failed(:final error) => FailurePanel(error, onRetry: vm.load),
          Loaded(:final value) when value.isEmpty => MessagePanel(icon: FluentIcons.info_24_regular, text: l.noData),
          Loaded(:final value) => ListView(
              children: [
                for (final area in value) ...[
                  SectionHeader(area.name, trailing: l.machineCount(area.machines.length)),
                  GridRow(header: true, [
                    GridCell(l.colMachine, flex: 3),
                    GridCell(l.lastRead, flex: 3),
                    GridCell(l.coinIn, flex: 4, alignEnd: true),
                    GridCell(l.machineWin, flex: 4, alignEnd: true),
                  ]),
                  for (final m in area.machines)
                    GridRow([
                      GridCell(m.assetNumber, flex: 3, strong: true),
                      GridCell(m.lastReadAt == null ? l.neverRead : f.time(m.lastReadAt!), flex: 3, muted: true),
                      GridCell(f.amountOnly(m.todayCoinIn), flex: 4, alignEnd: true),
                      GridCell(f.amountOnly(m.todayWin), flex: 4, alignEnd: true, color: signedColor(context, m.todayWin)),
                    ]),
                  for (final e in area.totals.entries)
                    GridRow(highlight: true, [
                      GridCell(e.key, flex: 6),
                      GridCell(f.amountOnly(e.value.coinIn), flex: 4, alignEnd: true),
                      GridCell(f.amountOnly(e.value.win), flex: 4, alignEnd: true, color: signedColor(context, e.value.win)),
                    ]),
                ],
              ],
            ),
        },
      ),
    );
  }
}
