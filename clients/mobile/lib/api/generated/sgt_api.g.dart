// GENERATED CODE - DO NOT MODIFY BY HAND.
// Kaynak: docs/api/openapi-v1.json · Üretici: tool/gen_api.dart
// Sözleşme değişince: dart run tool/gen_api.dart

// ignore_for_file: public_member_api_docs, constant_identifier_names

import 'package:decimal/decimal.dart';

import '../api_codec.dart';
import '../api_transport.dart';

class AddRestrictionRequest {
  const AddRestrictionRequest({
    required this.type,
    this.note,
    this.until,
    required this.reasonCode,
  });

  final RestrictionType type;
  final String? note;
  final DateTime? until;
  final String reasonCode;

  factory AddRestrictionRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return AddRestrictionRequest(
      type: RestrictionType.fromJson(m['type']),
      note: ApiCodec.nullable(m['note'], (v) => ApiCodec.string(v)),
      until: ApiCodec.nullable(m['until'], (v) => ApiCodec.dateTime(v)),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'type': type.toJson(),
        'note': note,
        'until': ApiCodec.nullable(until, (v) => ApiCodec.writeDateTime(v)),
        'reasonCode': reasonCode,
      };
}

class AdjustmentDecisionRequest {
  const AdjustmentDecisionRequest({
    required this.approve,
    required this.reasonCode,
  });

  final bool approve;
  final String reasonCode;

  factory AdjustmentDecisionRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return AdjustmentDecisionRequest(
      approve: ApiCodec.boolean(m['approve']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'approve': approve,
        'reasonCode': reasonCode,
      };
}

class AdjustmentRequest {
  const AdjustmentRequest({
    required this.transactionId,
    required this.note,
    required this.reasonCode,
  });

  final String transactionId;
  final String note;
  final String reasonCode;

  factory AdjustmentRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return AdjustmentRequest(
      transactionId: ApiCodec.string(m['transactionId']),
      note: ApiCodec.string(m['note']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'transactionId': transactionId,
        'note': note,
        'reasonCode': reasonCode,
      };
}

class AuditEntryView {
  const AuditEntryView({
    required this.id,
    required this.eventType,
    required this.summary,
    required this.actorId,
    required this.terminal,
    required this.reasonCode,
    required this.correlationId,
    required this.occurredAt,
  });

  final String id;
  final String eventType;
  final String summary;
  final String actorId;
  final String terminal;
  final String reasonCode;
  final String correlationId;
  final DateTime occurredAt;

  factory AuditEntryView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return AuditEntryView(
      id: ApiCodec.string(m['id']),
      eventType: ApiCodec.string(m['eventType']),
      summary: ApiCodec.string(m['summary']),
      actorId: ApiCodec.string(m['actorId']),
      terminal: ApiCodec.string(m['terminal']),
      reasonCode: ApiCodec.string(m['reasonCode']),
      correlationId: ApiCodec.string(m['correlationId']),
      occurredAt: ApiCodec.dateTime(m['occurredAt']),
    );
  }

  Map<String, Object?> toJson() => {
        'id': id,
        'eventType': eventType,
        'summary': summary,
        'actorId': actorId,
        'terminal': terminal,
        'reasonCode': reasonCode,
        'correlationId': correlationId,
        'occurredAt': ApiCodec.writeDateTime(occurredAt),
      };
}

class BalanceView {
  const BalanceView({
    required this.instrument,
    required this.currency,
    required this.expected,
  });

  final String instrument;
  final String currency;
  final Decimal expected;

  factory BalanceView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return BalanceView(
      instrument: ApiCodec.string(m['instrument']),
      currency: ApiCodec.string(m['currency']),
      expected: ApiCodec.decimal(m['expected']),
    );
  }

  Map<String, Object?> toJson() => {
        'instrument': instrument,
        'currency': currency,
        'expected': expected.toString(),
      };
}

class CageCurrencyTotals {
  const CageCurrencyTotals({
    required this.currency,
    required this.chipSales,
    required this.chipRedemptions,
    required this.foreignBought,
    required this.foreignSold,
    required this.closingVariance,
  });

  final String currency;
  final Decimal chipSales;
  final Decimal chipRedemptions;
  final Decimal foreignBought;
  final Decimal foreignSold;
  final Decimal closingVariance;

  factory CageCurrencyTotals.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return CageCurrencyTotals(
      currency: ApiCodec.string(m['currency']),
      chipSales: ApiCodec.decimal(m['chipSales']),
      chipRedemptions: ApiCodec.decimal(m['chipRedemptions']),
      foreignBought: ApiCodec.decimal(m['foreignBought']),
      foreignSold: ApiCodec.decimal(m['foreignSold']),
      closingVariance: ApiCodec.decimal(m['closingVariance']),
    );
  }

  Map<String, Object?> toJson() => {
        'currency': currency,
        'chipSales': chipSales.toString(),
        'chipRedemptions': chipRedemptions.toString(),
        'foreignBought': foreignBought.toString(),
        'foreignSold': foreignSold.toString(),
        'closingVariance': closingVariance.toString(),
      };
}

class CageRequest {
  const CageRequest({
    required this.transactionId,
    required this.playerId,
    required this.amount,
    required this.currency,
    required this.reasonCode,
  });

  final String transactionId;
  final String playerId;
  final Decimal amount;
  final String currency;
  final String reasonCode;

  factory CageRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return CageRequest(
      transactionId: ApiCodec.string(m['transactionId']),
      playerId: ApiCodec.string(m['playerId']),
      amount: ApiCodec.decimal(m['amount']),
      currency: ApiCodec.string(m['currency']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'transactionId': transactionId,
        'playerId': playerId,
        'amount': amount.toString(),
        'currency': currency,
        'reasonCode': reasonCode,
      };
}

class CageTransactionResult {
  const CageTransactionResult({
    required this.transactionId,
    required this.type,
    required this.amount,
    required this.currency,
    this.rate,
    required this.movements,
  });

  final String transactionId;
  final String type;
  final Decimal amount;
  final String currency;
  final Decimal? rate;
  final List<DrawerMovement> movements;

  factory CageTransactionResult.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return CageTransactionResult(
      transactionId: ApiCodec.string(m['transactionId']),
      type: ApiCodec.string(m['type']),
      amount: ApiCodec.decimal(m['amount']),
      currency: ApiCodec.string(m['currency']),
      rate: ApiCodec.nullable(m['rate'], (v) => ApiCodec.decimal(v)),
      movements: ApiCodec.list(m['movements'], (v) => DrawerMovement.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'transactionId': transactionId,
        'type': type,
        'amount': amount.toString(),
        'currency': currency,
        'rate': ApiCodec.nullable(rate, (v) => v.toString()),
        'movements': movements.map((v) => v.toJson()).toList(),
      };
}

class CloseShiftRequest {
  const CloseShiftRequest({
    required this.counted,
    this.note,
    required this.reasonCode,
  });

  final List<DrawerAmount> counted;
  final String? note;
  final String reasonCode;

  factory CloseShiftRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return CloseShiftRequest(
      counted: ApiCodec.list(m['counted'], (v) => DrawerAmount.fromJson(v)),
      note: ApiCodec.nullable(m['note'], (v) => ApiCodec.string(v)),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'counted': counted.map((v) => v.toJson()).toList(),
        'note': note,
        'reasonCode': reasonCode,
      };
}

class ClosingCountView {
  const ClosingCountView({
    required this.instrument,
    required this.currency,
    required this.expected,
    required this.counted,
    required this.variance,
  });

  final String instrument;
  final String currency;
  final Decimal expected;
  final Decimal counted;
  final Decimal variance;

  factory ClosingCountView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ClosingCountView(
      instrument: ApiCodec.string(m['instrument']),
      currency: ApiCodec.string(m['currency']),
      expected: ApiCodec.decimal(m['expected']),
      counted: ApiCodec.decimal(m['counted']),
      variance: ApiCodec.decimal(m['variance']),
    );
  }

  Map<String, Object?> toJson() => {
        'instrument': instrument,
        'currency': currency,
        'expected': expected.toString(),
        'counted': counted.toString(),
        'variance': variance.toString(),
      };
}

class CreateRoleRequest {
  const CreateRoleRequest({
    required this.name,
    required this.reasonCode,
  });

  final String name;
  final String reasonCode;

  factory CreateRoleRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return CreateRoleRequest(
      name: ApiCodec.string(m['name']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'name': name,
        'reasonCode': reasonCode,
      };
}

class CreatedId {
  const CreatedId({
    required this.id,
  });

  final String id;

  factory CreatedId.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return CreatedId(
      id: ApiCodec.string(m['id']),
    );
  }

  Map<String, Object?> toJson() => {
        'id': id,
      };
}

class DailyCageSummary {
  const DailyCageSummary({
    required this.shiftsOpen,
    required this.shiftsClosed,
    required this.reversals,
    required this.currencies,
  });

  final int shiftsOpen;
  final int shiftsClosed;
  final int reversals;
  final List<CageCurrencyTotals> currencies;

  factory DailyCageSummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DailyCageSummary(
      shiftsOpen: ApiCodec.integer(m['shiftsOpen']),
      shiftsClosed: ApiCodec.integer(m['shiftsClosed']),
      reversals: ApiCodec.integer(m['reversals']),
      currencies: ApiCodec.list(m['currencies'], (v) => CageCurrencyTotals.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'shiftsOpen': shiftsOpen,
        'shiftsClosed': shiftsClosed,
        'reversals': reversals,
        'currencies': currencies.map((v) => v.toJson()).toList(),
      };
}

class DailyFloorSummary {
  const DailyFloorSummary({
    required this.ratingsFinished,
    required this.live,
    required this.slots,
  });

  final int ratingsFinished;
  final List<LiveCurrencyTotals> live;
  final List<SlotCurrencyTotals> slots;

  factory DailyFloorSummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DailyFloorSummary(
      ratingsFinished: ApiCodec.integer(m['ratingsFinished']),
      live: ApiCodec.list(m['live'], (v) => LiveCurrencyTotals.fromJson(v)),
      slots: ApiCodec.list(m['slots'], (v) => SlotCurrencyTotals.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'ratingsFinished': ratingsFinished,
        'live': live.map((v) => v.toJson()).toList(),
        'slots': slots.map((v) => v.toJson()).toList(),
      };
}

class DailyLoyaltySummary {
  const DailyLoyaltySummary({
    required this.pointsEarned,
    required this.pointsRedeemed,
  });

  final int pointsEarned;
  final int pointsRedeemed;

  factory DailyLoyaltySummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DailyLoyaltySummary(
      pointsEarned: ApiCodec.integer(m['pointsEarned']),
      pointsRedeemed: ApiCodec.integer(m['pointsRedeemed']),
    );
  }

  Map<String, Object?> toJson() => {
        'pointsEarned': pointsEarned,
        'pointsRedeemed': pointsRedeemed,
      };
}

class DailySummary {
  const DailySummary({
    required this.date,
    required this.from,
    required this.to,
    required this.cage,
    required this.floor,
    required this.loyalty,
  });

  final DateTime date;
  final DateTime from;
  final DateTime to;
  final DailyCageSummary cage;
  final DailyFloorSummary floor;
  final DailyLoyaltySummary loyalty;

  factory DailySummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DailySummary(
      date: ApiCodec.date(m['date']),
      from: ApiCodec.dateTime(m['from']),
      to: ApiCodec.dateTime(m['to']),
      cage: DailyCageSummary.fromJson(m['cage']),
      floor: DailyFloorSummary.fromJson(m['floor']),
      loyalty: DailyLoyaltySummary.fromJson(m['loyalty']),
    );
  }

  Map<String, Object?> toJson() => {
        'date': ApiCodec.writeDate(date),
        'from': ApiCodec.writeDateTime(from),
        'to': ApiCodec.writeDateTime(to),
        'cage': cage.toJson(),
        'floor': floor.toJson(),
        'loyalty': loyalty.toJson(),
      };
}

class DeadLetter {
  const DeadLetter({
    required this.module,
    required this.eventId,
    required this.type,
    required this.attempts,
    this.lastError,
    required this.occurredAt,
  });

  final String module;
  final String eventId;
  final String type;
  final int attempts;
  final String? lastError;
  final DateTime occurredAt;

  factory DeadLetter.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DeadLetter(
      module: ApiCodec.string(m['module']),
      eventId: ApiCodec.string(m['eventId']),
      type: ApiCodec.string(m['type']),
      attempts: ApiCodec.integer(m['attempts']),
      lastError: ApiCodec.nullable(m['lastError'], (v) => ApiCodec.string(v)),
      occurredAt: ApiCodec.dateTime(m['occurredAt']),
    );
  }

  Map<String, Object?> toJson() => {
        'module': module,
        'eventId': eventId,
        'type': type,
        'attempts': attempts,
        'lastError': lastError,
        'occurredAt': ApiCodec.writeDateTime(occurredAt),
      };
}

class DecisionRequest {
  const DecisionRequest({
    required this.approve,
    required this.reasonCode,
  });

  final bool approve;
  final String reasonCode;

  factory DecisionRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DecisionRequest(
      approve: ApiCodec.boolean(m['approve']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'approve': approve,
        'reasonCode': reasonCode,
      };
}

class DefineSlotMachineRequest {
  const DefineSlotMachineRequest({
    required this.assetNumber,
    required this.area,
    required this.manufacturer,
    required this.model,
    required this.denomination,
    required this.currency,
    required this.reasonCode,
  });

  final String assetNumber;
  final String area;
  final String manufacturer;
  final String model;
  final Decimal denomination;
  final String currency;
  final String reasonCode;

  factory DefineSlotMachineRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DefineSlotMachineRequest(
      assetNumber: ApiCodec.string(m['assetNumber']),
      area: ApiCodec.string(m['area']),
      manufacturer: ApiCodec.string(m['manufacturer']),
      model: ApiCodec.string(m['model']),
      denomination: ApiCodec.decimal(m['denomination']),
      currency: ApiCodec.string(m['currency']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'assetNumber': assetNumber,
        'area': area,
        'manufacturer': manufacturer,
        'model': model,
        'denomination': denomination.toString(),
        'currency': currency,
        'reasonCode': reasonCode,
      };
}

class DefineTableRequest {
  const DefineTableRequest({
    required this.code,
    required this.gameType,
    required this.currency,
    required this.minBet,
    required this.maxBet,
    required this.houseEdgePercent,
    required this.handsPerHour,
    required this.reasonCode,
  });

  final String code;
  final GameType gameType;
  final String currency;
  final Decimal minBet;
  final Decimal maxBet;
  final Decimal houseEdgePercent;
  final int handsPerHour;
  final String reasonCode;

  factory DefineTableRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DefineTableRequest(
      code: ApiCodec.string(m['code']),
      gameType: GameType.fromJson(m['gameType']),
      currency: ApiCodec.string(m['currency']),
      minBet: ApiCodec.decimal(m['minBet']),
      maxBet: ApiCodec.decimal(m['maxBet']),
      houseEdgePercent: ApiCodec.decimal(m['houseEdgePercent']),
      handsPerHour: ApiCodec.integer(m['handsPerHour']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'code': code,
        'gameType': gameType.toJson(),
        'currency': currency,
        'minBet': minBet.toString(),
        'maxBet': maxBet.toString(),
        'houseEdgePercent': houseEdgePercent.toString(),
        'handsPerHour': handsPerHour,
        'reasonCode': reasonCode,
      };
}

enum DocumentType {
  nationalId('NationalId'),
  passport('Passport'),
  driverLicense('DriverLicense'),
  ;

  const DocumentType(this.wire);

  final String wire;

  static DocumentType fromJson(Object? value) => ApiCodec.enumValue(value, values, (e) => e.wire);

  String toJson() => wire;
}

class DrawerAmount {
  const DrawerAmount({
    required this.instrument,
    required this.currency,
    required this.amount,
  });

  final Instrument instrument;
  final String currency;
  final Decimal amount;

  factory DrawerAmount.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DrawerAmount(
      instrument: Instrument.fromJson(m['instrument']),
      currency: ApiCodec.string(m['currency']),
      amount: ApiCodec.decimal(m['amount']),
    );
  }

  Map<String, Object?> toJson() => {
        'instrument': instrument.toJson(),
        'currency': currency,
        'amount': amount.toString(),
      };
}

class DrawerMovement {
  const DrawerMovement({
    required this.instrument,
    required this.currency,
    required this.amount,
  });

  final String instrument;
  final String currency;
  final Decimal amount;

  factory DrawerMovement.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return DrawerMovement(
      instrument: ApiCodec.string(m['instrument']),
      currency: ApiCodec.string(m['currency']),
      amount: ApiCodec.decimal(m['amount']),
    );
  }

  Map<String, Object?> toJson() => {
        'instrument': instrument,
        'currency': currency,
        'amount': amount.toString(),
      };
}

class EarningRuleRequest {
  const EarningRuleRequest({
    required this.tryPerPoint,
    required this.reasonCode,
  });

  final Decimal tryPerPoint;
  final String reasonCode;

  factory EarningRuleRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return EarningRuleRequest(
      tryPerPoint: ApiCodec.decimal(m['tryPerPoint']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'tryPerPoint': tryPerPoint.toString(),
        'reasonCode': reasonCode,
      };
}

class ExchangeRateDto {
  const ExchangeRateDto({
    required this.currency,
    required this.buy,
    required this.sell,
    required this.effectiveFrom,
  });

  final String currency;
  final Decimal buy;
  final Decimal sell;
  final DateTime effectiveFrom;

  factory ExchangeRateDto.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ExchangeRateDto(
      currency: ApiCodec.string(m['currency']),
      buy: ApiCodec.decimal(m['buy']),
      sell: ApiCodec.decimal(m['sell']),
      effectiveFrom: ApiCodec.dateTime(m['effectiveFrom']),
    );
  }

  Map<String, Object?> toJson() => {
        'currency': currency,
        'buy': buy.toString(),
        'sell': sell.toString(),
        'effectiveFrom': ApiCodec.writeDateTime(effectiveFrom),
      };
}

class FloorReasonRequest {
  const FloorReasonRequest({
    required this.reasonCode,
  });

  final String reasonCode;

  factory FloorReasonRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return FloorReasonRequest(
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'reasonCode': reasonCode,
      };
}

enum GameType {
  blackjack('Blackjack'),
  roulette('Roulette'),
  poker('Poker'),
  baccarat('Baccarat'),
  other('Other'),
  ;

  const GameType(this.wire);

  final String wire;

  static GameType fromJson(Object? value) => ApiCodec.enumValue(value, values, (e) => e.wire);

  String toJson() => wire;
}

class ImportLineError {
  const ImportLineError({
    required this.line,
    required this.code,
    required this.message,
  });

  final int line;
  final String code;
  final String message;

  factory ImportLineError.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ImportLineError(
      line: ApiCodec.integer(m['line']),
      code: ApiCodec.string(m['code']),
      message: ApiCodec.string(m['message']),
    );
  }

  Map<String, Object?> toJson() => {
        'line': line,
        'code': code,
        'message': message,
      };
}

class ImportReport {
  const ImportReport({
    required this.imported,
    required this.errors,
  });

  final int imported;
  final List<ImportLineError> errors;

  factory ImportReport.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ImportReport(
      imported: ApiCodec.integer(m['imported']),
      errors: ApiCodec.list(m['errors'], (v) => ImportLineError.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'imported': imported,
        'errors': errors.map((v) => v.toJson()).toList(),
      };
}

class ImportRequest {
  const ImportRequest({
    required this.csv,
    required this.reasonCode,
  });

  final String csv;
  final String reasonCode;

  factory ImportRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ImportRequest(
      csv: ApiCodec.string(m['csv']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'csv': csv,
        'reasonCode': reasonCode,
      };
}

enum Instrument {
  cash('Cash'),
  chips('Chips'),
  ;

  const Instrument(this.wire);

  final String wire;

  static Instrument fromJson(Object? value) => ApiCodec.enumValue(value, values, (e) => e.wire);

  String toJson() => wire;
}

class IssueCardRequest {
  const IssueCardRequest({
    required this.cardNumber,
    required this.reasonCode,
  });

  final String cardNumber;
  final String reasonCode;

  factory IssueCardRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return IssueCardRequest(
      cardNumber: ApiCodec.string(m['cardNumber']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'cardNumber': cardNumber,
        'reasonCode': reasonCode,
      };
}

class LiveCurrencyTotals {
  const LiveCurrencyTotals({
    required this.currency,
    required this.theoreticalWin,
    required this.casinoResult,
  });

  final String currency;
  final Decimal theoreticalWin;
  final Decimal casinoResult;

  factory LiveCurrencyTotals.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return LiveCurrencyTotals(
      currency: ApiCodec.string(m['currency']),
      theoreticalWin: ApiCodec.decimal(m['theoreticalWin']),
      casinoResult: ApiCodec.decimal(m['casinoResult']),
    );
  }

  Map<String, Object?> toJson() => {
        'currency': currency,
        'theoreticalWin': theoreticalWin.toString(),
        'casinoResult': casinoResult.toString(),
      };
}

class LoyaltySummary {
  const LoyaltySummary({
    required this.playerId,
    required this.balance,
    required this.lifetimeEarned,
    required this.tier,
    required this.recentEntries,
  });

  final String playerId;
  final int balance;
  final int lifetimeEarned;
  final String tier;
  final List<PointsEntryView> recentEntries;

  factory LoyaltySummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return LoyaltySummary(
      playerId: ApiCodec.string(m['playerId']),
      balance: ApiCodec.integer(m['balance']),
      lifetimeEarned: ApiCodec.integer(m['lifetimeEarned']),
      tier: ApiCodec.string(m['tier']),
      recentEntries: ApiCodec.list(m['recentEntries'], (v) => PointsEntryView.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'playerId': playerId,
        'balance': balance,
        'lifetimeEarned': lifetimeEarned,
        'tier': tier,
        'recentEntries': recentEntries.map((v) => v.toJson()).toList(),
      };
}

class MenuItem {
  const MenuItem({
    required this.screen,
    required this.actions,
  });

  final String screen;
  final List<String> actions;

  factory MenuItem.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return MenuItem(
      screen: ApiCodec.string(m['screen']),
      actions: ApiCodec.list(m['actions'], (v) => ApiCodec.string(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'screen': screen,
        'actions': actions.map((v) => v).toList(),
      };
}

class MeterReadingRequest {
  const MeterReadingRequest({
    required this.assetNumber,
    required this.readAt,
    required this.coinIn,
    required this.coinOut,
    required this.jackpot,
    required this.gamesPlayed,
    required this.isReset,
    this.note,
    required this.reference,
    required this.reasonCode,
  });

  final String assetNumber;
  final DateTime readAt;
  final Decimal coinIn;
  final Decimal coinOut;
  final Decimal jackpot;
  final int gamesPlayed;
  final bool isReset;
  final String? note;
  final String reference;
  final String reasonCode;

  factory MeterReadingRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return MeterReadingRequest(
      assetNumber: ApiCodec.string(m['assetNumber']),
      readAt: ApiCodec.dateTime(m['readAt']),
      coinIn: ApiCodec.decimal(m['coinIn']),
      coinOut: ApiCodec.decimal(m['coinOut']),
      jackpot: ApiCodec.decimal(m['jackpot']),
      gamesPlayed: ApiCodec.integer(m['gamesPlayed']),
      isReset: ApiCodec.boolean(m['isReset']),
      note: ApiCodec.nullable(m['note'], (v) => ApiCodec.string(v)),
      reference: ApiCodec.string(m['reference']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'assetNumber': assetNumber,
        'readAt': ApiCodec.writeDateTime(readAt),
        'coinIn': coinIn.toString(),
        'coinOut': coinOut.toString(),
        'jackpot': jackpot.toString(),
        'gamesPlayed': gamesPlayed,
        'isReset': isReset,
        'note': note,
        'reference': reference,
        'reasonCode': reasonCode,
      };
}

class MeterSnapshot {
  const MeterSnapshot({
    required this.coinIn,
    required this.coinOut,
    required this.jackpot,
    required this.gamesPlayed,
    this.machineWin,
    this.isValid,
  });

  final Decimal coinIn;
  final Decimal coinOut;
  final Decimal jackpot;
  final int gamesPlayed;
  final Decimal? machineWin;
  final bool? isValid;

  factory MeterSnapshot.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return MeterSnapshot(
      coinIn: ApiCodec.decimal(m['coinIn']),
      coinOut: ApiCodec.decimal(m['coinOut']),
      jackpot: ApiCodec.decimal(m['jackpot']),
      gamesPlayed: ApiCodec.integer(m['gamesPlayed']),
      machineWin: ApiCodec.nullable(m['machineWin'], (v) => ApiCodec.decimal(v)),
      isValid: ApiCodec.nullable(m['isValid'], (v) => ApiCodec.boolean(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'coinIn': coinIn.toString(),
        'coinOut': coinOut.toString(),
        'jackpot': jackpot.toString(),
        'gamesPlayed': gamesPlayed,
        'machineWin': ApiCodec.nullable(machineWin, (v) => v.toString()),
        'isValid': isValid,
      };
}

class OpenShiftRequest {
  const OpenShiftRequest({
    required this.opening,
    required this.reasonCode,
  });

  final List<DrawerAmount> opening;
  final String reasonCode;

  factory OpenShiftRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return OpenShiftRequest(
      opening: ApiCodec.list(m['opening'], (v) => DrawerAmount.fromJson(v)),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'opening': opening.map((v) => v.toJson()).toList(),
        'reasonCode': reasonCode,
      };
}

class PermissionChangeView {
  const PermissionChangeView({
    required this.id,
    required this.target,
    required this.targetId,
    required this.kind,
    required this.item,
    required this.actorId,
    required this.terminal,
    required this.reasonCode,
    required this.correlationId,
    required this.occurredAt,
  });

  final String id;
  final String target;
  final String targetId;
  final String kind;
  final String item;
  final String actorId;
  final String terminal;
  final String reasonCode;
  final String correlationId;
  final DateTime occurredAt;

  factory PermissionChangeView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PermissionChangeView(
      id: ApiCodec.string(m['id']),
      target: ApiCodec.string(m['target']),
      targetId: ApiCodec.string(m['targetId']),
      kind: ApiCodec.string(m['kind']),
      item: ApiCodec.string(m['item']),
      actorId: ApiCodec.string(m['actorId']),
      terminal: ApiCodec.string(m['terminal']),
      reasonCode: ApiCodec.string(m['reasonCode']),
      correlationId: ApiCodec.string(m['correlationId']),
      occurredAt: ApiCodec.dateTime(m['occurredAt']),
    );
  }

  Map<String, Object?> toJson() => {
        'id': id,
        'target': target,
        'targetId': targetId,
        'kind': kind,
        'item': item,
        'actorId': actorId,
        'terminal': terminal,
        'reasonCode': reasonCode,
        'correlationId': correlationId,
        'occurredAt': ApiCodec.writeDateTime(occurredAt),
      };
}

class PermissionRequest {
  const PermissionRequest({
    required this.permission,
    required this.reasonCode,
  });

  final String permission;
  final String reasonCode;

  factory PermissionRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PermissionRequest(
      permission: ApiCodec.string(m['permission']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'permission': permission,
        'reasonCode': reasonCode,
      };
}

class PlayerDetailsRequest {
  const PlayerDetailsRequest({
    required this.firstName,
    required this.lastName,
    required this.birthDate,
    required this.nationality,
    required this.documentType,
    required this.documentNumber,
    this.phone,
    required this.reasonCode,
  });

  final String firstName;
  final String lastName;
  final DateTime birthDate;
  final String nationality;
  final DocumentType documentType;
  final String documentNumber;
  final String? phone;
  final String reasonCode;

  factory PlayerDetailsRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PlayerDetailsRequest(
      firstName: ApiCodec.string(m['firstName']),
      lastName: ApiCodec.string(m['lastName']),
      birthDate: ApiCodec.date(m['birthDate']),
      nationality: ApiCodec.string(m['nationality']),
      documentType: DocumentType.fromJson(m['documentType']),
      documentNumber: ApiCodec.string(m['documentNumber']),
      phone: ApiCodec.nullable(m['phone'], (v) => ApiCodec.string(v)),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'firstName': firstName,
        'lastName': lastName,
        'birthDate': ApiCodec.writeDate(birthDate),
        'nationality': nationality,
        'documentType': documentType.toJson(),
        'documentNumber': documentNumber,
        'phone': phone,
        'reasonCode': reasonCode,
      };
}

class PlayerLocationDto {
  const PlayerLocationDto({
    required this.playerId,
    required this.tableCode,
    required this.since,
  });

  final String playerId;
  final String tableCode;
  final DateTime since;

  factory PlayerLocationDto.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PlayerLocationDto(
      playerId: ApiCodec.string(m['playerId']),
      tableCode: ApiCodec.string(m['tableCode']),
      since: ApiCodec.dateTime(m['since']),
    );
  }

  Map<String, Object?> toJson() => {
        'playerId': playerId,
        'tableCode': tableCode,
        'since': ApiCodec.writeDateTime(since),
      };
}

class PlayerReasonRequest {
  const PlayerReasonRequest({
    required this.reasonCode,
  });

  final String reasonCode;

  factory PlayerReasonRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PlayerReasonRequest(
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'reasonCode': reasonCode,
      };
}

class PlayerRegistered {
  const PlayerRegistered({
    required this.playerId,
    required this.playerNumber,
  });

  final String playerId;
  final String playerNumber;

  factory PlayerRegistered.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PlayerRegistered(
      playerId: ApiCodec.string(m['playerId']),
      playerNumber: ApiCodec.string(m['playerNumber']),
    );
  }

  Map<String, Object?> toJson() => {
        'playerId': playerId,
        'playerNumber': playerNumber,
      };
}

class PlayerSummary {
  const PlayerSummary({
    required this.playerId,
    required this.playerNumber,
    required this.firstName,
    required this.lastName,
    this.activeCardNumber,
    required this.isRestricted,
  });

  final String playerId;
  final String playerNumber;
  final String firstName;
  final String lastName;
  final String? activeCardNumber;
  final bool isRestricted;

  factory PlayerSummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PlayerSummary(
      playerId: ApiCodec.string(m['playerId']),
      playerNumber: ApiCodec.string(m['playerNumber']),
      firstName: ApiCodec.string(m['firstName']),
      lastName: ApiCodec.string(m['lastName']),
      activeCardNumber: ApiCodec.nullable(m['activeCardNumber'], (v) => ApiCodec.string(v)),
      isRestricted: ApiCodec.boolean(m['isRestricted']),
    );
  }

  Map<String, Object?> toJson() => {
        'playerId': playerId,
        'playerNumber': playerNumber,
        'firstName': firstName,
        'lastName': lastName,
        'activeCardNumber': activeCardNumber,
        'isRestricted': isRestricted,
      };
}

class PlayerView {
  const PlayerView({
    required this.playerId,
    required this.playerNumber,
    required this.firstName,
    required this.lastName,
    required this.birthDate,
    required this.nationality,
    required this.documentType,
    required this.maskedDocumentNumber,
    this.phone,
    this.activeCardNumber,
    required this.isRestricted,
    required this.activeRestrictions,
  });

  final String playerId;
  final String playerNumber;
  final String firstName;
  final String lastName;
  final DateTime birthDate;
  final String nationality;
  final String documentType;
  final String maskedDocumentNumber;
  final String? phone;
  final String? activeCardNumber;
  final bool isRestricted;
  final List<RestrictionView> activeRestrictions;

  factory PlayerView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PlayerView(
      playerId: ApiCodec.string(m['playerId']),
      playerNumber: ApiCodec.string(m['playerNumber']),
      firstName: ApiCodec.string(m['firstName']),
      lastName: ApiCodec.string(m['lastName']),
      birthDate: ApiCodec.date(m['birthDate']),
      nationality: ApiCodec.string(m['nationality']),
      documentType: ApiCodec.string(m['documentType']),
      maskedDocumentNumber: ApiCodec.string(m['maskedDocumentNumber']),
      phone: ApiCodec.nullable(m['phone'], (v) => ApiCodec.string(v)),
      activeCardNumber: ApiCodec.nullable(m['activeCardNumber'], (v) => ApiCodec.string(v)),
      isRestricted: ApiCodec.boolean(m['isRestricted']),
      activeRestrictions: ApiCodec.list(m['activeRestrictions'], (v) => RestrictionView.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'playerId': playerId,
        'playerNumber': playerNumber,
        'firstName': firstName,
        'lastName': lastName,
        'birthDate': ApiCodec.writeDate(birthDate),
        'nationality': nationality,
        'documentType': documentType,
        'maskedDocumentNumber': maskedDocumentNumber,
        'phone': phone,
        'activeCardNumber': activeCardNumber,
        'isRestricted': isRestricted,
        'activeRestrictions': activeRestrictions.map((v) => v.toJson()).toList(),
      };
}

class PointsEntryView {
  const PointsEntryView({
    required this.kind,
    required this.points,
    required this.sourceId,
    required this.at,
  });

  final String kind;
  final int points;
  final String sourceId;
  final DateTime at;

  factory PointsEntryView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PointsEntryView(
      kind: ApiCodec.string(m['kind']),
      points: ApiCodec.integer(m['points']),
      sourceId: ApiCodec.string(m['sourceId']),
      at: ApiCodec.dateTime(m['at']),
    );
  }

  Map<String, Object?> toJson() => {
        'kind': kind,
        'points': points,
        'sourceId': sourceId,
        'at': ApiCodec.writeDateTime(at),
      };
}

class PointsRequest {
  const PointsRequest({
    required this.requestId,
    required this.playerId,
    required this.points,
    required this.text,
    required this.reasonCode,
  });

  final String requestId;
  final String playerId;
  final int points;
  final String text;
  final String reasonCode;

  factory PointsRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return PointsRequest(
      requestId: ApiCodec.string(m['requestId']),
      playerId: ApiCodec.string(m['playerId']),
      points: ApiCodec.integer(m['points']),
      text: ApiCodec.string(m['text']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'requestId': requestId,
        'playerId': playerId,
        'points': points,
        'text': text,
        'reasonCode': reasonCode,
      };
}

class ProblemDetails {
  const ProblemDetails({
    this.type,
    this.title,
    this.status,
    this.detail,
    this.instance,
  });

  final String? type;
  final String? title;
  final int? status;
  final String? detail;
  final String? instance;

  factory ProblemDetails.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ProblemDetails(
      type: ApiCodec.nullable(m['type'], (v) => ApiCodec.string(v)),
      title: ApiCodec.nullable(m['title'], (v) => ApiCodec.string(v)),
      status: ApiCodec.nullable(m['status'], (v) => ApiCodec.integer(v)),
      detail: ApiCodec.nullable(m['detail'], (v) => ApiCodec.string(v)),
      instance: ApiCodec.nullable(m['instance'], (v) => ApiCodec.string(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'type': type,
        'title': title,
        'status': status,
        'detail': detail,
        'instance': instance,
      };
}

class RatingValuesRequest {
  const RatingValuesRequest({
    required this.averageBet,
    this.playerResult,
    required this.reasonCode,
  });

  final Decimal averageBet;
  final Decimal? playerResult;
  final String reasonCode;

  factory RatingValuesRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return RatingValuesRequest(
      averageBet: ApiCodec.decimal(m['averageBet']),
      playerResult: ApiCodec.nullable(m['playerResult'], (v) => ApiCodec.decimal(v)),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'averageBet': averageBet.toString(),
        'playerResult': ApiCodec.nullable(playerResult, (v) => v.toString()),
        'reasonCode': reasonCode,
      };
}

class ReadingView {
  const ReadingView({
    required this.readAt,
    required this.source,
    required this.meters,
    required this.delta,
    required this.deltaWin,
    required this.isReset,
    this.note,
  });

  final DateTime readAt;
  final String source;
  final MeterSnapshot meters;
  final MeterSnapshot delta;
  final Decimal deltaWin;
  final bool isReset;
  final String? note;

  factory ReadingView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ReadingView(
      readAt: ApiCodec.dateTime(m['readAt']),
      source: ApiCodec.string(m['source']),
      meters: MeterSnapshot.fromJson(m['meters']),
      delta: MeterSnapshot.fromJson(m['delta']),
      deltaWin: ApiCodec.decimal(m['deltaWin']),
      isReset: ApiCodec.boolean(m['isReset']),
      note: ApiCodec.nullable(m['note'], (v) => ApiCodec.string(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'readAt': ApiCodec.writeDateTime(readAt),
        'source': source,
        'meters': meters.toJson(),
        'delta': delta.toJson(),
        'deltaWin': deltaWin.toString(),
        'isReset': isReset,
        'note': note,
      };
}

class ReasonRequest {
  const ReasonRequest({
    required this.reasonCode,
  });

  final String reasonCode;

  factory ReasonRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ReasonRequest(
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'reasonCode': reasonCode,
      };
}

class RegisterUserRequest {
  const RegisterUserRequest({
    required this.userId,
    required this.displayName,
    required this.employeeCode,
    required this.reasonCode,
  });

  final String userId;
  final String displayName;
  final String employeeCode;
  final String reasonCode;

  factory RegisterUserRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return RegisterUserRequest(
      userId: ApiCodec.string(m['userId']),
      displayName: ApiCodec.string(m['displayName']),
      employeeCode: ApiCodec.string(m['employeeCode']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'userId': userId,
        'displayName': displayName,
        'employeeCode': employeeCode,
        'reasonCode': reasonCode,
      };
}

class ReprocessRequest {
  const ReprocessRequest({
    required this.reasonCode,
  });

  final String reasonCode;

  factory ReprocessRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ReprocessRequest(
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'reasonCode': reasonCode,
      };
}

enum RestrictionType {
  banned('Banned'),
  selfExcluded('SelfExcluded'),
  ;

  const RestrictionType(this.wire);

  final String wire;

  static RestrictionType fromJson(Object? value) => ApiCodec.enumValue(value, values, (e) => e.wire);

  String toJson() => wire;
}

class RestrictionView {
  const RestrictionView({
    required this.id,
    required this.type,
    this.note,
    this.until,
    required this.addedBy,
    required this.addedAt,
  });

  final String id;
  final String type;
  final String? note;
  final DateTime? until;
  final String addedBy;
  final DateTime addedAt;

  factory RestrictionView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return RestrictionView(
      id: ApiCodec.string(m['id']),
      type: ApiCodec.string(m['type']),
      note: ApiCodec.nullable(m['note'], (v) => ApiCodec.string(v)),
      until: ApiCodec.nullable(m['until'], (v) => ApiCodec.dateTime(v)),
      addedBy: ApiCodec.string(m['addedBy']),
      addedAt: ApiCodec.dateTime(m['addedAt']),
    );
  }

  Map<String, Object?> toJson() => {
        'id': id,
        'type': type,
        'note': note,
        'until': ApiCodec.nullable(until, (v) => ApiCodec.writeDateTime(v)),
        'addedBy': addedBy,
        'addedAt': ApiCodec.writeDateTime(addedAt),
      };
}

class RoleAssignmentRequest {
  const RoleAssignmentRequest({
    required this.roleId,
    required this.reasonCode,
  });

  final String roleId;
  final String reasonCode;

  factory RoleAssignmentRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return RoleAssignmentRequest(
      roleId: ApiCodec.string(m['roleId']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'roleId': roleId,
        'reasonCode': reasonCode,
      };
}

class RoleView {
  const RoleView({
    required this.id,
    required this.name,
    required this.permissions,
  });

  final String id;
  final String name;
  final List<String> permissions;

  factory RoleView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return RoleView(
      id: ApiCodec.string(m['id']),
      name: ApiCodec.string(m['name']),
      permissions: ApiCodec.list(m['permissions'], (v) => ApiCodec.string(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'id': id,
        'name': name,
        'permissions': permissions.map((v) => v).toList(),
      };
}

class RunningRatingView {
  const RunningRatingView({
    required this.ratingId,
    required this.playerId,
    this.playerNumber,
    required this.tableId,
    required this.tableCode,
    required this.startedAt,
  });

  final String ratingId;
  final String playerId;
  final String? playerNumber;
  final String tableId;
  final String tableCode;
  final DateTime startedAt;

  factory RunningRatingView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return RunningRatingView(
      ratingId: ApiCodec.string(m['ratingId']),
      playerId: ApiCodec.string(m['playerId']),
      playerNumber: ApiCodec.nullable(m['playerNumber'], (v) => ApiCodec.string(v)),
      tableId: ApiCodec.string(m['tableId']),
      tableCode: ApiCodec.string(m['tableCode']),
      startedAt: ApiCodec.dateTime(m['startedAt']),
    );
  }

  Map<String, Object?> toJson() => {
        'ratingId': ratingId,
        'playerId': playerId,
        'playerNumber': playerNumber,
        'tableId': tableId,
        'tableCode': tableCode,
        'startedAt': ApiCodec.writeDateTime(startedAt),
      };
}

class ScreenView {
  const ScreenView({
    required this.code,
    required this.product,
    required this.actions,
  });

  final String code;
  final String product;
  final List<String> actions;

  factory ScreenView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ScreenView(
      code: ApiCodec.string(m['code']),
      product: ApiCodec.string(m['product']),
      actions: ApiCodec.list(m['actions'], (v) => ApiCodec.string(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'code': code,
        'product': product,
        'actions': actions.map((v) => v).toList(),
      };
}

class SetExchangeRateRequest {
  const SetExchangeRateRequest({
    required this.currency,
    required this.buy,
    required this.sell,
    required this.reasonCode,
  });

  final String currency;
  final Decimal buy;
  final Decimal sell;
  final String reasonCode;

  factory SetExchangeRateRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return SetExchangeRateRequest(
      currency: ApiCodec.string(m['currency']),
      buy: ApiCodec.decimal(m['buy']),
      sell: ApiCodec.decimal(m['sell']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'currency': currency,
        'buy': buy.toString(),
        'sell': sell.toString(),
        'reasonCode': reasonCode,
      };
}

class ShiftSummary {
  const ShiftSummary({
    required this.shiftId,
    required this.cashierId,
    required this.terminal,
    required this.openedAt,
    this.closedAt,
    this.closingNote,
    required this.balances,
    required this.transactions,
    required this.closingCounts,
  });

  final String shiftId;
  final String cashierId;
  final String terminal;
  final DateTime openedAt;
  final DateTime? closedAt;
  final String? closingNote;
  final List<BalanceView> balances;
  final List<CageTransactionResult> transactions;
  final List<ClosingCountView> closingCounts;

  factory ShiftSummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return ShiftSummary(
      shiftId: ApiCodec.string(m['shiftId']),
      cashierId: ApiCodec.string(m['cashierId']),
      terminal: ApiCodec.string(m['terminal']),
      openedAt: ApiCodec.dateTime(m['openedAt']),
      closedAt: ApiCodec.nullable(m['closedAt'], (v) => ApiCodec.dateTime(v)),
      closingNote: ApiCodec.nullable(m['closingNote'], (v) => ApiCodec.string(v)),
      balances: ApiCodec.list(m['balances'], (v) => BalanceView.fromJson(v)),
      transactions: ApiCodec.list(m['transactions'], (v) => CageTransactionResult.fromJson(v)),
      closingCounts: ApiCodec.list(m['closingCounts'], (v) => ClosingCountView.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'shiftId': shiftId,
        'cashierId': cashierId,
        'terminal': terminal,
        'openedAt': ApiCodec.writeDateTime(openedAt),
        'closedAt': ApiCodec.nullable(closedAt, (v) => ApiCodec.writeDateTime(v)),
        'closingNote': closingNote,
        'balances': balances.map((v) => v.toJson()).toList(),
        'transactions': transactions.map((v) => v.toJson()).toList(),
        'closingCounts': closingCounts.map((v) => v.toJson()).toList(),
      };
}

class SlotCurrencyTotals {
  const SlotCurrencyTotals({
    required this.currency,
    required this.coinIn,
    required this.machineWin,
  });

  final String currency;
  final Decimal coinIn;
  final Decimal machineWin;

  factory SlotCurrencyTotals.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return SlotCurrencyTotals(
      currency: ApiCodec.string(m['currency']),
      coinIn: ApiCodec.decimal(m['coinIn']),
      machineWin: ApiCodec.decimal(m['machineWin']),
    );
  }

  Map<String, Object?> toJson() => {
        'currency': currency,
        'coinIn': coinIn.toString(),
        'machineWin': machineWin.toString(),
      };
}

class SlotFloorRow {
  const SlotFloorRow({
    required this.machineId,
    required this.assetNumber,
    required this.area,
    required this.currency,
    this.lastReadAt,
    required this.todayCoinIn,
    required this.todayWin,
  });

  final String machineId;
  final String assetNumber;
  final String area;
  final String currency;
  final DateTime? lastReadAt;
  final Decimal todayCoinIn;
  final Decimal todayWin;

  factory SlotFloorRow.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return SlotFloorRow(
      machineId: ApiCodec.string(m['machineId']),
      assetNumber: ApiCodec.string(m['assetNumber']),
      area: ApiCodec.string(m['area']),
      currency: ApiCodec.string(m['currency']),
      lastReadAt: ApiCodec.nullable(m['lastReadAt'], (v) => ApiCodec.dateTime(v)),
      todayCoinIn: ApiCodec.decimal(m['todayCoinIn']),
      todayWin: ApiCodec.decimal(m['todayWin']),
    );
  }

  Map<String, Object?> toJson() => {
        'machineId': machineId,
        'assetNumber': assetNumber,
        'area': area,
        'currency': currency,
        'lastReadAt': ApiCodec.nullable(lastReadAt, (v) => ApiCodec.writeDateTime(v)),
        'todayCoinIn': todayCoinIn.toString(),
        'todayWin': todayWin.toString(),
      };
}

class SlotMachineView {
  const SlotMachineView({
    required this.machineId,
    required this.assetNumber,
    required this.area,
    required this.manufacturer,
    required this.model,
    required this.denomination,
    required this.currency,
    required this.readings,
  });

  final String machineId;
  final String assetNumber;
  final String area;
  final String manufacturer;
  final String model;
  final Decimal denomination;
  final String currency;
  final List<ReadingView> readings;

  factory SlotMachineView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return SlotMachineView(
      machineId: ApiCodec.string(m['machineId']),
      assetNumber: ApiCodec.string(m['assetNumber']),
      area: ApiCodec.string(m['area']),
      manufacturer: ApiCodec.string(m['manufacturer']),
      model: ApiCodec.string(m['model']),
      denomination: ApiCodec.decimal(m['denomination']),
      currency: ApiCodec.string(m['currency']),
      readings: ApiCodec.list(m['readings'], (v) => ReadingView.fromJson(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'machineId': machineId,
        'assetNumber': assetNumber,
        'area': area,
        'manufacturer': manufacturer,
        'model': model,
        'denomination': denomination.toString(),
        'currency': currency,
        'readings': readings.map((v) => v.toJson()).toList(),
      };
}

class StartRatingRequest {
  const StartRatingRequest({
    required this.tableId,
    required this.playerId,
    required this.reasonCode,
  });

  final String tableId;
  final String playerId;
  final String reasonCode;

  factory StartRatingRequest.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return StartRatingRequest(
      tableId: ApiCodec.string(m['tableId']),
      playerId: ApiCodec.string(m['playerId']),
      reasonCode: ApiCodec.string(m['reasonCode']),
    );
  }

  Map<String, Object?> toJson() => {
        'tableId': tableId,
        'playerId': playerId,
        'reasonCode': reasonCode,
      };
}

class TableView {
  const TableView({
    required this.tableId,
    required this.code,
    required this.gameType,
    required this.currency,
    required this.minBet,
    required this.maxBet,
    required this.houseEdgePercent,
    required this.handsPerHour,
    required this.isOpen,
  });

  final String tableId;
  final String code;
  final String gameType;
  final String currency;
  final Decimal minBet;
  final Decimal maxBet;
  final Decimal houseEdgePercent;
  final int handsPerHour;
  final bool isOpen;

  factory TableView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return TableView(
      tableId: ApiCodec.string(m['tableId']),
      code: ApiCodec.string(m['code']),
      gameType: ApiCodec.string(m['gameType']),
      currency: ApiCodec.string(m['currency']),
      minBet: ApiCodec.decimal(m['minBet']),
      maxBet: ApiCodec.decimal(m['maxBet']),
      houseEdgePercent: ApiCodec.decimal(m['houseEdgePercent']),
      handsPerHour: ApiCodec.integer(m['handsPerHour']),
      isOpen: ApiCodec.boolean(m['isOpen']),
    );
  }

  Map<String, Object?> toJson() => {
        'tableId': tableId,
        'code': code,
        'gameType': gameType,
        'currency': currency,
        'minBet': minBet.toString(),
        'maxBet': maxBet.toString(),
        'houseEdgePercent': houseEdgePercent.toString(),
        'handsPerHour': handsPerHour,
        'isOpen': isOpen,
      };
}

class UserAccessView {
  const UserAccessView({
    required this.userId,
    required this.isSuperAdmin,
    required this.roleIds,
    required this.grants,
    required this.denied,
    required this.effective,
  });

  final String userId;
  final bool isSuperAdmin;
  final List<String> roleIds;
  final List<String> grants;
  final List<String> denied;
  final List<String> effective;

  factory UserAccessView.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return UserAccessView(
      userId: ApiCodec.string(m['userId']),
      isSuperAdmin: ApiCodec.boolean(m['isSuperAdmin']),
      roleIds: ApiCodec.list(m['roleIds'], (v) => ApiCodec.string(v)),
      grants: ApiCodec.list(m['grants'], (v) => ApiCodec.string(v)),
      denied: ApiCodec.list(m['denied'], (v) => ApiCodec.string(v)),
      effective: ApiCodec.list(m['effective'], (v) => ApiCodec.string(v)),
    );
  }

  Map<String, Object?> toJson() => {
        'userId': userId,
        'isSuperAdmin': isSuperAdmin,
        'roleIds': roleIds.map((v) => v).toList(),
        'grants': grants.map((v) => v).toList(),
        'denied': denied.map((v) => v).toList(),
        'effective': effective.map((v) => v).toList(),
      };
}

class UserSummary {
  const UserSummary({
    required this.userId,
    required this.displayName,
    required this.employeeCode,
    required this.isActive,
    required this.isSuperAdmin,
  });

  final String userId;
  final String displayName;
  final String employeeCode;
  final bool isActive;
  final bool isSuperAdmin;

  factory UserSummary.fromJson(Object? json) {
    final m = ApiCodec.object(json);
    return UserSummary(
      userId: ApiCodec.string(m['userId']),
      displayName: ApiCodec.string(m['displayName']),
      employeeCode: ApiCodec.string(m['employeeCode']),
      isActive: ApiCodec.boolean(m['isActive']),
      isSuperAdmin: ApiCodec.boolean(m['isSuperAdmin']),
    );
  }

  Map<String, Object?> toJson() => {
        'userId': userId,
        'displayName': displayName,
        'employeeCode': employeeCode,
        'isActive': isActive,
        'isSuperAdmin': isSuperAdmin,
      };
}

/// Tipli API istemcisi. Her metot bir uca karşılık gelir; ad = HTTP metodu + yol.
class SgtApiClient {
  const SgtApiClient(this._transport);

  final ApiTransport _transport;

  /// GET /api/v1/audit/entries
  Future<List<AuditEntryView>> getAuditEntries({DateTime? from, DateTime? to, String? actorId, String? correlationId, int? take}) async {
    final response = await _transport.send('GET', '/api/v1/audit/entries', query: {'from': ApiCodec.nullable(from, (v) => ApiCodec.writeDateTime(v).toString()), 'to': ApiCodec.nullable(to, (v) => ApiCodec.writeDateTime(v).toString()), 'actorId': actorId, 'correlationId': correlationId, 'take': ApiCodec.nullable(take, (v) => v.toString())});
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => AuditEntryView.fromJson(v)));
  }

  /// POST /api/v1/cage/adjustments/{requestId}/decision
  Future<void> postCageAdjustmentsByRequestIdDecision({required String requestId, required AdjustmentDecisionRequest body}) async {
    await _transport.send('POST', '/api/v1/cage/adjustments/${Uri.encodeComponent(requestId.toString())}/decision', body: body.toJson());
  }

  /// POST /api/v1/cage/shifts
  Future<CreatedId> postCageShifts({required OpenShiftRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/cage/shifts', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// GET /api/v1/cage/shifts/mine
  Future<CreatedId?> getCageShiftsMine() async {
    final response = await _transport.send('GET', '/api/v1/cage/shifts/mine');
    return ApiCodec.decode(() => ApiCodec.nullable(response.body, (v) => CreatedId.fromJson(v)));
  }

  /// GET /api/v1/cage/shifts/{shiftId}
  Future<ShiftSummary> getCageShiftsByShiftId({required String shiftId}) async {
    final response = await _transport.send('GET', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}');
    return ApiCodec.decode(() => ShiftSummary.fromJson(response.body));
  }

  /// POST /api/v1/cage/shifts/{shiftId}/adjustments
  Future<CreatedId> postCageShiftsByShiftIdAdjustments({required String shiftId, required AdjustmentRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}/adjustments', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/cage/shifts/{shiftId}/chip-redemptions
  Future<CageTransactionResult> postCageShiftsByShiftIdChipRedemptions({required String shiftId, required CageRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}/chip-redemptions', body: body.toJson());
    return ApiCodec.decode(() => CageTransactionResult.fromJson(response.body));
  }

  /// POST /api/v1/cage/shifts/{shiftId}/chip-sales
  Future<CageTransactionResult> postCageShiftsByShiftIdChipSales({required String shiftId, required CageRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}/chip-sales', body: body.toJson());
    return ApiCodec.decode(() => CageTransactionResult.fromJson(response.body));
  }

  /// POST /api/v1/cage/shifts/{shiftId}/close
  Future<void> postCageShiftsByShiftIdClose({required String shiftId, required CloseShiftRequest body}) async {
    await _transport.send('POST', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}/close', body: body.toJson());
  }

  /// POST /api/v1/cage/shifts/{shiftId}/exchanges/base-to-foreign
  Future<CageTransactionResult> postCageShiftsByShiftIdExchangesBaseToForeign({required String shiftId, required CageRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}/exchanges/base-to-foreign', body: body.toJson());
    return ApiCodec.decode(() => CageTransactionResult.fromJson(response.body));
  }

  /// POST /api/v1/cage/shifts/{shiftId}/exchanges/foreign-to-base
  Future<CageTransactionResult> postCageShiftsByShiftIdExchangesForeignToBase({required String shiftId, required CageRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/cage/shifts/${Uri.encodeComponent(shiftId.toString())}/exchanges/foreign-to-base', body: body.toJson());
    return ApiCodec.decode(() => CageTransactionResult.fromJson(response.body));
  }

  /// POST /api/v1/configuration/exchange-rates
  Future<void> postConfigurationExchangeRates({required SetExchangeRateRequest body}) async {
    await _transport.send('POST', '/api/v1/configuration/exchange-rates', body: body.toJson());
  }

  /// GET /api/v1/configuration/exchange-rates/current
  Future<List<ExchangeRateDto>> getConfigurationExchangeRatesCurrent() async {
    final response = await _transport.send('GET', '/api/v1/configuration/exchange-rates/current');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => ExchangeRateDto.fromJson(v)));
  }

  /// GET /api/v1/floor/players/{playerId}/location
  Future<PlayerLocationDto?> getFloorPlayersByPlayerIdLocation({required String playerId}) async {
    final response = await _transport.send('GET', '/api/v1/floor/players/${Uri.encodeComponent(playerId.toString())}/location');
    return ApiCodec.decode(() => ApiCodec.nullable(response.body, (v) => PlayerLocationDto.fromJson(v)));
  }

  /// POST /api/v1/floor/ratings
  Future<CreatedId> postFloorRatings({required StartRatingRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/floor/ratings', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// GET /api/v1/floor/ratings/running
  Future<List<RunningRatingView>> getFloorRatingsRunning({String? tableId}) async {
    final response = await _transport.send('GET', '/api/v1/floor/ratings/running', query: {'tableId': tableId});
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => RunningRatingView.fromJson(v)));
  }

  /// POST /api/v1/floor/ratings/{ratingId}/correct
  Future<void> postFloorRatingsByRatingIdCorrect({required String ratingId, required RatingValuesRequest body}) async {
    await _transport.send('POST', '/api/v1/floor/ratings/${Uri.encodeComponent(ratingId.toString())}/correct', body: body.toJson());
  }

  /// POST /api/v1/floor/ratings/{ratingId}/finish
  Future<void> postFloorRatingsByRatingIdFinish({required String ratingId, required RatingValuesRequest body}) async {
    await _transport.send('POST', '/api/v1/floor/ratings/${Uri.encodeComponent(ratingId.toString())}/finish', body: body.toJson());
  }

  /// GET /api/v1/floor/slots
  Future<List<SlotFloorRow>> getFloorSlots() async {
    final response = await _transport.send('GET', '/api/v1/floor/slots');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => SlotFloorRow.fromJson(v)));
  }

  /// POST /api/v1/floor/slots
  Future<CreatedId> postFloorSlots({required DefineSlotMachineRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/floor/slots', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/floor/slots/readings
  Future<void> postFloorSlotsReadings({required MeterReadingRequest body}) async {
    await _transport.send('POST', '/api/v1/floor/slots/readings', body: body.toJson());
  }

  /// POST /api/v1/floor/slots/readings/import
  Future<ImportReport> postFloorSlotsReadingsImport({required ImportRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/floor/slots/readings/import', body: body.toJson());
    return ApiCodec.decode(() => ImportReport.fromJson(response.body));
  }

  /// GET /api/v1/floor/slots/{machineId}
  Future<SlotMachineView> getFloorSlotsByMachineId({required String machineId}) async {
    final response = await _transport.send('GET', '/api/v1/floor/slots/${Uri.encodeComponent(machineId.toString())}');
    return ApiCodec.decode(() => SlotMachineView.fromJson(response.body));
  }

  /// GET /api/v1/floor/tables
  Future<List<TableView>> getFloorTables() async {
    final response = await _transport.send('GET', '/api/v1/floor/tables');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => TableView.fromJson(v)));
  }

  /// POST /api/v1/floor/tables
  Future<CreatedId> postFloorTables({required DefineTableRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/floor/tables', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/floor/tables/{tableId}/close
  Future<void> postFloorTablesByTableIdClose({required String tableId, required FloorReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/floor/tables/${Uri.encodeComponent(tableId.toString())}/close', body: body.toJson());
  }

  /// POST /api/v1/floor/tables/{tableId}/open
  Future<void> postFloorTablesByTableIdOpen({required String tableId, required FloorReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/floor/tables/${Uri.encodeComponent(tableId.toString())}/open', body: body.toJson());
  }

  /// GET /api/v1/identity/me/menu
  Future<List<MenuItem>> getIdentityMeMenu() async {
    final response = await _transport.send('GET', '/api/v1/identity/me/menu');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => MenuItem.fromJson(v)));
  }

  /// GET /api/v1/identity/permission-changes
  Future<List<PermissionChangeView>> getIdentityPermissionChanges({int? take, DateTime? before}) async {
    final response = await _transport.send('GET', '/api/v1/identity/permission-changes', query: {'take': ApiCodec.nullable(take, (v) => v.toString()), 'before': ApiCodec.nullable(before, (v) => ApiCodec.writeDateTime(v).toString())});
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => PermissionChangeView.fromJson(v)));
  }

  /// GET /api/v1/identity/roles
  Future<List<RoleView>> getIdentityRoles() async {
    final response = await _transport.send('GET', '/api/v1/identity/roles');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => RoleView.fromJson(v)));
  }

  /// POST /api/v1/identity/roles
  Future<CreatedId> postIdentityRoles({required CreateRoleRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/identity/roles', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/identity/roles/defaults
  Future<void> postIdentityRolesDefaults({required ReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/roles/defaults', body: body.toJson());
  }

  /// POST /api/v1/identity/roles/{roleId}/permissions
  Future<void> postIdentityRolesByRoleIdPermissions({required String roleId, required PermissionRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/roles/${Uri.encodeComponent(roleId.toString())}/permissions', body: body.toJson());
  }

  /// DELETE /api/v1/identity/roles/{roleId}/permissions/{permission}
  Future<void> deleteIdentityRolesByRoleIdPermissionsByPermission({required String roleId, required String permission, String? reasonCode}) async {
    await _transport.send('DELETE', '/api/v1/identity/roles/${Uri.encodeComponent(roleId.toString())}/permissions/${Uri.encodeComponent(permission.toString())}', query: {'reasonCode': reasonCode});
  }

  /// GET /api/v1/identity/screens
  Future<List<ScreenView>> getIdentityScreens() async {
    final response = await _transport.send('GET', '/api/v1/identity/screens');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => ScreenView.fromJson(v)));
  }

  /// GET /api/v1/identity/users
  Future<List<UserSummary>> getIdentityUsers() async {
    final response = await _transport.send('GET', '/api/v1/identity/users');
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => UserSummary.fromJson(v)));
  }

  /// POST /api/v1/identity/users
  Future<void> postIdentityUsers({required RegisterUserRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/users', body: body.toJson());
  }

  /// GET /api/v1/identity/users/{userId}/access
  Future<UserAccessView> getIdentityUsersByUserIdAccess({required String userId}) async {
    final response = await _transport.send('GET', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/access');
    return ApiCodec.decode(() => UserAccessView.fromJson(response.body));
  }

  /// POST /api/v1/identity/users/{userId}/deactivate
  Future<void> postIdentityUsersByUserIdDeactivate({required String userId, required ReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/deactivate', body: body.toJson());
  }

  /// POST /api/v1/identity/users/{userId}/denies
  Future<void> postIdentityUsersByUserIdDenies({required String userId, required PermissionRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/denies', body: body.toJson());
  }

  /// DELETE /api/v1/identity/users/{userId}/denies/{permission}
  Future<void> deleteIdentityUsersByUserIdDeniesByPermission({required String userId, required String permission, String? reasonCode}) async {
    await _transport.send('DELETE', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/denies/${Uri.encodeComponent(permission.toString())}', query: {'reasonCode': reasonCode});
  }

  /// POST /api/v1/identity/users/{userId}/grants
  Future<void> postIdentityUsersByUserIdGrants({required String userId, required PermissionRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/grants', body: body.toJson());
  }

  /// DELETE /api/v1/identity/users/{userId}/grants/{permission}
  Future<void> deleteIdentityUsersByUserIdGrantsByPermission({required String userId, required String permission, String? reasonCode}) async {
    await _transport.send('DELETE', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/grants/${Uri.encodeComponent(permission.toString())}', query: {'reasonCode': reasonCode});
  }

  /// POST /api/v1/identity/users/{userId}/reactivate
  Future<void> postIdentityUsersByUserIdReactivate({required String userId, required ReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/reactivate', body: body.toJson());
  }

  /// POST /api/v1/identity/users/{userId}/roles
  Future<void> postIdentityUsersByUserIdRoles({required String userId, required RoleAssignmentRequest body}) async {
    await _transport.send('POST', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/roles', body: body.toJson());
  }

  /// DELETE /api/v1/identity/users/{userId}/roles/{roleId}
  Future<void> deleteIdentityUsersByUserIdRolesByRoleId({required String userId, required String roleId, String? reasonCode}) async {
    await _transport.send('DELETE', '/api/v1/identity/users/${Uri.encodeComponent(userId.toString())}/roles/${Uri.encodeComponent(roleId.toString())}', query: {'reasonCode': reasonCode});
  }

  /// GET /api/v1/integration/dead-letters
  Future<List<DeadLetter>> getIntegrationDeadLetters({int? take}) async {
    final response = await _transport.send('GET', '/api/v1/integration/dead-letters', query: {'take': ApiCodec.nullable(take, (v) => v.toString())});
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => DeadLetter.fromJson(v)));
  }

  /// POST /api/v1/integration/dead-letters/{module}/{eventId}/reprocess
  Future<void> postIntegrationDeadLettersByModuleByEventIdReprocess({required String module, required String eventId, required ReprocessRequest body}) async {
    await _transport.send('POST', '/api/v1/integration/dead-letters/${Uri.encodeComponent(module.toString())}/${Uri.encodeComponent(eventId.toString())}/reprocess', body: body.toJson());
  }

  /// POST /api/v1/loyalty/adjustments
  Future<CreatedId> postLoyaltyAdjustments({required PointsRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/loyalty/adjustments', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/loyalty/adjustments/{requestId}/decision
  Future<void> postLoyaltyAdjustmentsByRequestIdDecision({required String requestId, required DecisionRequest body}) async {
    await _transport.send('POST', '/api/v1/loyalty/adjustments/${Uri.encodeComponent(requestId.toString())}/decision', body: body.toJson());
  }

  /// POST /api/v1/loyalty/comps
  Future<CreatedId> postLoyaltyComps({required PointsRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/loyalty/comps', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/loyalty/comps/{requestId}/decision
  Future<void> postLoyaltyCompsByRequestIdDecision({required String requestId, required DecisionRequest body}) async {
    await _transport.send('POST', '/api/v1/loyalty/comps/${Uri.encodeComponent(requestId.toString())}/decision', body: body.toJson());
  }

  /// GET /api/v1/loyalty/players/{playerId}
  Future<LoyaltySummary> getLoyaltyPlayersByPlayerId({required String playerId}) async {
    final response = await _transport.send('GET', '/api/v1/loyalty/players/${Uri.encodeComponent(playerId.toString())}');
    return ApiCodec.decode(() => LoyaltySummary.fromJson(response.body));
  }

  /// PUT /api/v1/loyalty/rules/earning
  Future<void> putLoyaltyRulesEarning({required EarningRuleRequest body}) async {
    await _transport.send('PUT', '/api/v1/loyalty/rules/earning', body: body.toJson());
  }

  /// GET /api/v1/players
  Future<List<PlayerSummary>> getPlayers({String? q}) async {
    final response = await _transport.send('GET', '/api/v1/players', query: {'q': q});
    return ApiCodec.decode(() => ApiCodec.list(response.body, (v) => PlayerSummary.fromJson(v)));
  }

  /// POST /api/v1/players
  Future<PlayerRegistered> postPlayers({required PlayerDetailsRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/players', body: body.toJson());
    return ApiCodec.decode(() => PlayerRegistered.fromJson(response.body));
  }

  /// GET /api/v1/players/{playerId}
  Future<PlayerView> getPlayersByPlayerId({required String playerId}) async {
    final response = await _transport.send('GET', '/api/v1/players/${Uri.encodeComponent(playerId.toString())}');
    return ApiCodec.decode(() => PlayerView.fromJson(response.body));
  }

  /// PUT /api/v1/players/{playerId}
  Future<void> putPlayersByPlayerId({required String playerId, required PlayerDetailsRequest body}) async {
    await _transport.send('PUT', '/api/v1/players/${Uri.encodeComponent(playerId.toString())}', body: body.toJson());
  }

  /// POST /api/v1/players/{playerId}/cards
  Future<void> postPlayersByPlayerIdCards({required String playerId, required IssueCardRequest body}) async {
    await _transport.send('POST', '/api/v1/players/${Uri.encodeComponent(playerId.toString())}/cards', body: body.toJson());
  }

  /// POST /api/v1/players/{playerId}/cards/cancel
  Future<void> postPlayersByPlayerIdCardsCancel({required String playerId, required PlayerReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/players/${Uri.encodeComponent(playerId.toString())}/cards/cancel', body: body.toJson());
  }

  /// POST /api/v1/players/{playerId}/restrictions
  Future<CreatedId> postPlayersByPlayerIdRestrictions({required String playerId, required AddRestrictionRequest body}) async {
    final response = await _transport.send('POST', '/api/v1/players/${Uri.encodeComponent(playerId.toString())}/restrictions', body: body.toJson());
    return ApiCodec.decode(() => CreatedId.fromJson(response.body));
  }

  /// POST /api/v1/players/{playerId}/restrictions/{restrictionId}/remove
  Future<void> postPlayersByPlayerIdRestrictionsByRestrictionIdRemove({required String playerId, required String restrictionId, required PlayerReasonRequest body}) async {
    await _transport.send('POST', '/api/v1/players/${Uri.encodeComponent(playerId.toString())}/restrictions/${Uri.encodeComponent(restrictionId.toString())}/remove', body: body.toJson());
  }

  /// GET /api/v1/reports/daily
  Future<DailySummary> getReportsDaily({DateTime? date}) async {
    final response = await _transport.send('GET', '/api/v1/reports/daily', query: {'date': ApiCodec.nullable(date, (v) => ApiCodec.writeDate(v).toString())});
    return ApiCodec.decode(() => DailySummary.fromJson(response.body));
  }
}
