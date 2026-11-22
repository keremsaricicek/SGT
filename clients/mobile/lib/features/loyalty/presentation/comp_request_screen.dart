import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/services.dart';
import 'package:material_ui/material_ui.dart';

import '../../../api/generated/sgt_api.g.dart';
import '../../../app/services.dart';
import '../../../theme/sgt_tokens.dart';
import '../../../ui/sgt_widgets.dart';
import '../../../ui/player_header.dart';
import 'comp_request_view_model.dart';

/// Comp talebi (L2, KK9). Onay ekranı yok: bekleyen talepleri listeleyen API ucu yok (spec 014, eksikler).
class CompRequestScreen extends StatefulWidget {
  const CompRequestScreen({super.key, required this.playerId, this.summary, this.initial});

  final String playerId;
  final PlayerSummary? summary;

  /// Golden/test için başlangıç değerleri (puan, açıklama, gerekçe).
  final (String, String, String)? initial;

  @override
  State<CompRequestScreen> createState() => _CompRequestScreenState();
}

class _CompRequestScreenState extends State<CompRequestScreen> {
  CompRequestViewModel? _vm;
  final _points = TextEditingController();
  final _text = TextEditingController();
  final _reason = TextEditingController();

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_vm == null) {
      final s = AppServices.of(context);
      _vm = CompRequestViewModel(playerId: widget.playerId, repository: s.loyalty, newId: s.newId);
      final init = widget.initial;
      if (init != null) {
        _points.text = init.$1;
        _text.text = init.$2;
        _reason.text = init.$3;
        _vm!
          ..setPoints(init.$1)
          ..setText(init.$2)
          ..setReason(init.$3);
      }
    }
  }

  @override
  void dispose() {
    _vm?.dispose();
    _points.dispose();
    _text.dispose();
    _reason.dispose();
    super.dispose();
  }

  String? _errorText(CompField f) {
    final l = context.l10n;
    return switch (_vm!.errors[f]) {
      null => null,
      CompFieldError.required => l.errRequired,
      CompFieldError.pointsPositive => l.errPointsPositive,
      CompFieldError.reasonLength => l.errReasonLength,
    };
  }

  @override
  Widget build(BuildContext context) {
    final vm = _vm!;
    final l = context.l10n;
    return ListenableBuilder(
      listenable: vm,
      builder: (context, _) => SgtPage(
        title: l.screenCompRequest,
        bottomAction: FilledButton.icon(
          onPressed: vm.state is CompSending ? null : vm.submit,
          icon: const Icon(FluentIcons.send_24_regular, size: 18),
          label: Text(l.submitRequest),
        ),
        body: ListView(
          children: [
            PlayerHeader(widget.summary),
            SectionHeader(l.screenCompRequest),
            Padding(
              padding: const EdgeInsets.all(SgtTokens.space3),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  TextField(
                    controller: _points,
                    keyboardType: TextInputType.number,
                    inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9-]'))],
                    decoration: InputDecoration(labelText: l.fieldPoints, errorText: _errorText(CompField.points)),
                    onChanged: vm.setPoints,
                  ),
                  const SizedBox(height: SgtTokens.space3),
                  TextField(
                    controller: _text,
                    maxLength: 200,
                    decoration: InputDecoration(labelText: l.fieldText, errorText: _errorText(CompField.text), counterText: ''),
                    onChanged: vm.setText,
                  ),
                  const SizedBox(height: SgtTokens.space3),
                  TextField(
                    controller: _reason,
                    textCapitalization: TextCapitalization.characters,
                    autocorrect: false,
                    decoration: InputDecoration(
                      labelText: l.fieldReason,
                      helperText: l.fieldReasonHelp,
                      errorText: _errorText(CompField.reason),
                    ),
                    onChanged: vm.setReason,
                  ),
                ],
              ),
            ),
            switch (vm.state) {
              CompEditing() => const SizedBox.shrink(),
              CompSending() => const LoadingPanel(),
              CompSent() => MessagePanel(icon: FluentIcons.checkmark_circle_24_regular, text: l.compSent, tone: TagTone.positive),
              CompFailed(:final error, :final retrySafe) => MessagePanel(
                  icon: FluentIcons.error_circle_24_regular,
                  tone: TagTone.negative,
                  text: retrySafe ? '${failureText(context, error)}\n${l.compRetryHint}' : failureText(context, error),
                ),
            },
          ],
        ),
      ),
    );
  }
}
