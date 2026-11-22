import '../api/generated/sgt_api.g.dart';

/// Özellikler arası sözleşme (K2): bir oyuncunun puan özetini okumak.
/// Oyuncu ekranı loyalty özelliğine doğrudan bağlanmaz; uygulama düzeyinde (AppServices) loyalty'nin
/// uygulaması bu sözleşmeye verilir.
abstract interface class LoyaltyPointsReader {
  Future<LoyaltySummary> summary(String playerId);
}
