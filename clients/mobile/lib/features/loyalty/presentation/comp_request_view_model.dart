import 'package:flutter/foundation.dart';

import '../../../api/api_exception.dart';
import '../../../api/generated/sgt_api.g.dart';
import '../domain/loyalty_repository.dart';

enum CompField { points, text, reason }

enum CompFieldError { required, pointsPositive, reasonLength }

sealed class CompSubmitState {
  const CompSubmitState();
}

final class CompEditing extends CompSubmitState {
  const CompEditing();
}

final class CompSending extends CompSubmitState {
  const CompSending();
}

final class CompSent extends CompSubmitState {
  const CompSent(this.requestId);

  final String requestId;
}

final class CompFailed extends CompSubmitState {
  const CompFailed(this.error);

  final ApiException error;

  /// Ağ hatasında aynı requestId ile güvenle tekrar gönderilebilir.
  bool get retrySafe => error.kind == ApiFailure.network || error.kind == ApiFailure.server;
}

/// Comp talebi (L2, KK9). İstemci kaynaklı idempotency: aynı bilgilerle tekrar gönderimde aynı requestId.
class CompRequestViewModel extends ChangeNotifier {
  CompRequestViewModel({required this.playerId, required this._repository, required this._newId});

  static const reasonMaxLength = 50;

  final String playerId;
  final LoyaltyRepository _repository;
  final String Function() _newId;

  String _points = '';
  String _text = '';
  String _reason = '';
  Map<CompField, CompFieldError> _errors = const {};
  CompSubmitState _state = const CompEditing();

  String? _pendingId;
  String? _pendingFingerprint;

  Map<CompField, CompFieldError> get errors => _errors;

  CompSubmitState get state => _state;

  String get normalizedReason => _reason.trim().toUpperCase();

  void setPoints(String v) => _edit(() => _points = v);

  void setText(String v) => _edit(() => _text = v);

  void setReason(String v) => _edit(() => _reason = v);

  Future<void> submit() async {
    if (_state is CompSending) return;
    final points = int.tryParse(_points.trim());
    final errors = <CompField, CompFieldError>{
      if (_points.trim().isEmpty)
        CompField.points: CompFieldError.required
      else if (points == null || points <= 0)
        CompField.points: CompFieldError.pointsPositive,
      if (_text.trim().isEmpty) CompField.text: CompFieldError.required,
      if (normalizedReason.isEmpty)
        CompField.reason: CompFieldError.required
      else if (normalizedReason.length > reasonMaxLength)
        CompField.reason: CompFieldError.reasonLength,
    };
    _errors = errors;
    if (errors.isNotEmpty) {
      notifyListeners();
      return;
    }

    final fingerprint = '$points|${_text.trim()}|$normalizedReason';
    if (_pendingFingerprint != fingerprint) {
      _pendingId = _newId();
      _pendingFingerprint = fingerprint;
    }
    final requestId = _pendingId!;
    _state = const CompSending();
    notifyListeners();
    try {
      await _repository.requestComp(PointsRequest(
        requestId: requestId,
        playerId: playerId,
        points: points!,
        text: _text.trim(),
        reasonCode: normalizedReason,
      ));
      _state = CompSent(requestId);
      _pendingId = null;
      _pendingFingerprint = null;
    } on ApiException catch (e) {
      _state = CompFailed(e);
    }
    notifyListeners();
  }

  void _edit(void Function() change) {
    change();
    if (_errors.isNotEmpty || _state is CompSent) {
      _errors = const {};
      if (_state is CompSent) _state = const CompEditing();
      notifyListeners();
    }
  }
}
