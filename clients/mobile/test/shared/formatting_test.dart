import 'package:decimal/decimal.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sgt_mobile/shared/formatting.dart';
import 'package:timezone/data/latest.dart' as tzdata;

void main() {
  setUpAll(tzdata.initializeTimeZones);

  const tr = SgtFormat('tr');
  const en = SgtFormat('en');

  test('para: Decimal, 2 ondalık, dile göre ayraç, para birimi kodu (KK13)', () {
    expect(tr.money(Decimal.parse('1845000'), 'TRY'), '1.845.000,00 TRY');
    expect(en.money(Decimal.parse('1845000'), 'TRY'), '1,845,000.00 TRY');
    expect(tr.money(Decimal.parse('-2300.005'), 'EUR'), '-2.300,01 EUR');
    expect(tr.money(Decimal.parse('0.1') + Decimal.parse('0.2'), 'GBP'), '0,30 GBP');
    expect(tr.money(Decimal.parse('-0.001'), 'USD'), '0,00 USD');
  });

  test('masa limiti tam sayıysa ondalıksız, değilse 2 ondalık', () {
    expect(tr.limit(Decimal.parse('25000')), '25.000');
    expect(tr.limit(Decimal.parse('2.5')), '2,50');
  });

  test('tam sayı gruplanır', () {
    expect(tr.integer(84210), '84.210');
    expect(en.integer(-1500), '-1,500');
  });

  test('zaman KKTC saatiyle gösterilir (DB UTC)', () {
    expect(tr.dateTime(DateTime.utc(2026, 11, 21, 17, 40)), '21.11.2026 19:40'); // kış: UTC+2
    expect(en.time(DateTime.utc(2026, 7, 1, 17, 40)), '20:40'); // yaz: UTC+3
  });
}
