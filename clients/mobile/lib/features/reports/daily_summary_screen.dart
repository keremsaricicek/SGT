import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:material_ui/material_ui.dart';

import '../../api/generated/sgt_api.g.dart';
import '../../app/services.dart';
import '../../shared/load_state.dart';
import '../../theme/sgt_theme.dart';
import '../../theme/sgt_tokens.dart';
import '../../ui/sgt_widgets.dart';
import 'daily_summary_view_model.dart';

/// Günlük casino özeti (R1, KK10).
class DailySummaryScreen extends StatefulWidget {
  const DailySummaryScreen({super.key});

  @override
  State<DailySummaryScreen> createState() => _DailySummaryScreenState();
}

class _DailySummaryScreenState extends State<DailySummaryScreen> {
  DailySummaryViewModel? _vm;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _vm ??= DailySummaryViewModel(AppServices.of(context).reports)..load();
  }

  @override
  void dispose() {
    _vm?.dispose();
    super.dispose();
  }

  Future<void> _pick(DateTime current) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: current,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
    );
    if (picked != null) await _vm!.load(picked);
  }

  @override
  Widget build(BuildContext context) {
    final vm = _vm!;
    final l = context.l10n;
    return SgtPage(
      title: l.screenDailySummary,
      actions: [IconButton(tooltip: l.refresh, icon: const Icon(FluentIcons.arrow_clockwise_24_regular), onPressed: vm.refresh)],
      body: ListenableBuilder(
        listenable: vm,
        builder: (context, _) => switch (vm.summary) {
          Idle() || Loading() => const LoadingPanel(),
          Failed(:final error) => FailurePanel(error, onRetry: vm.refresh),
          Loaded(:final value) => _Summary(
              value,
              isToday: vm.date == null,
              onPrevious: () => vm.load(value.date.subtract(const Duration(days: 1))),
              onNext: vm.date == null ? null : () => vm.load(value.date.add(const Duration(days: 1))),
              onPick: () => _pick(value.date),
              onToday: vm.date == null ? null : () => vm.load(),
            ),
        },
      ),
    );
  }
}

class _Summary extends StatelessWidget {
  const _Summary(this.s, {required this.isToday, required this.onPrevious, this.onNext, required this.onPick, this.onToday});

  final DailySummary s;
  final bool isToday;
  final VoidCallback onPrevious;
  final VoidCallback? onNext;
  final VoidCallback onPick;
  final VoidCallback? onToday;

  @override
  Widget build(BuildContext context) {
    final l = context.l10n;
    final f = context.fmt;
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    return ListView(
      children: [
        Container(
          color: p.surface,
          padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space1, vertical: SgtTokens.space1),
          child: Row(
            children: [
              IconButton(onPressed: onPrevious, icon: const Icon(FluentIcons.chevron_left_24_regular)),
              Expanded(
                child: InkWell(
                  onTap: onPick,
                  child: Column(
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(FluentIcons.calendar_ltr_24_regular, size: 16, color: p.textMuted),
                          const SizedBox(width: SgtTokens.space1),
                          Text(f.date(s.date), style: t.titleMedium),
                          if (isToday) ...[const SizedBox(width: SgtTokens.space2), StatusTag(l.today.toUpperCase())],
                        ],
                      ),
                      Text('${f.dateTime(s.from)} – ${f.dateTime(s.to)}', style: t.bodySmall),
                    ],
                  ),
                ),
              ),
              IconButton(onPressed: onNext, icon: const Icon(FluentIcons.chevron_right_24_regular)),
            ],
          ),
        ),
        if (onToday != null)
          Padding(
            padding: const EdgeInsets.fromLTRB(SgtTokens.space3, 0, SgtTokens.space3, SgtTokens.space2),
            child: OutlinedButton(onPressed: onToday, child: Text(l.today)),
          ),

        // Kasa
        SectionHeader(l.sectionCage),
        _Counts([
          (l.shiftsOpen, f.integer(s.cage.shiftsOpen)),
          (l.shiftsClosed, f.integer(s.cage.shiftsClosed)),
          (l.reversals, f.integer(s.cage.reversals)),
        ]),
        if (s.cage.currencies.isEmpty)
          _NoData()
        else ...[
          GridRow(header: true, [
            GridCell(l.currency, flex: 2),
            GridCell(l.chipSales, flex: 4, alignEnd: true),
            GridCell(l.chipRedemptions, flex: 4, alignEnd: true),
            GridCell(l.closingVariance, flex: 3, alignEnd: true),
          ]),
          for (final c in s.cage.currencies)
            GridRow([
              GridCell(c.currency, flex: 2, strong: true),
              GridCell(f.amountOnly(c.chipSales), flex: 4, alignEnd: true),
              GridCell(f.amountOnly(c.chipRedemptions), flex: 4, alignEnd: true),
              GridCell(f.amountOnly(c.closingVariance), flex: 3, alignEnd: true, color: signedColor(context, c.closingVariance)),
            ]),
        ],

        // Canlı oyun
        SectionHeader(l.sectionLive, trailing: '${l.ratingsFinished}: ${f.integer(s.floor.ratingsFinished)}'),
        if (s.floor.live.isEmpty)
          _NoData()
        else ...[
          GridRow(header: true, [
            GridCell(l.currency, flex: 2),
            GridCell(l.theoreticalWin, flex: 5, alignEnd: true),
            GridCell(l.casinoResult, flex: 5, alignEnd: true),
          ]),
          for (final c in s.floor.live)
            GridRow([
              GridCell(c.currency, flex: 2, strong: true),
              GridCell(f.amountOnly(c.theoreticalWin), flex: 5, alignEnd: true),
              GridCell(f.amountOnly(c.casinoResult), flex: 5, alignEnd: true, strong: true, color: signedColor(context, c.casinoResult)),
            ]),
        ],

        // Slot
        SectionHeader(l.sectionSlot),
        if (s.floor.slots.isEmpty)
          _NoData()
        else ...[
          GridRow(header: true, [
            GridCell(l.currency, flex: 2),
            GridCell(l.coinIn, flex: 5, alignEnd: true),
            GridCell(l.machineWin, flex: 5, alignEnd: true),
          ]),
          for (final c in s.floor.slots)
            GridRow([
              GridCell(c.currency, flex: 2, strong: true),
              GridCell(f.amountOnly(c.coinIn), flex: 5, alignEnd: true),
              GridCell(f.amountOnly(c.machineWin), flex: 5, alignEnd: true, strong: true, color: signedColor(context, c.machineWin)),
            ]),
        ],

        // Loyalty
        SectionHeader(l.areaLoyalty),
        ColoredBox(
          color: p.surface,
          child: Column(
            children: [
              KeyValueRow(l.pointsEarned, f.integer(s.loyalty.pointsEarned)),
              KeyValueRow(l.pointsRedeemed, f.integer(s.loyalty.pointsRedeemed)),
            ],
          ),
        ),
      ],
    );
  }
}

/// Yan yana sayaçlar (vardiya, ters kayıt).
class _Counts extends StatelessWidget {
  const _Counts(this.items);

  final List<(String, String)> items;

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final t = Theme.of(context).textTheme;
    return Container(
      decoration: BoxDecoration(color: p.surface, border: Border(bottom: BorderSide(color: p.border))),
      child: IntrinsicHeight(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            for (var i = 0; i < items.length; i++)
              Expanded(
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: SgtTokens.space3, vertical: SgtTokens.space2),
                  decoration: BoxDecoration(border: i == 0 ? null : Border(left: BorderSide(color: p.border))),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(items[i].$1, style: t.bodySmall),
                      Text(items[i].$2, style: t.headlineSmall),
                    ],
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _NoData extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Container(
        color: context.palette.surface,
        padding: const EdgeInsets.all(SgtTokens.space3),
        child: Text(context.l10n.noData, style: Theme.of(context).textTheme.bodySmall),
      );
}
