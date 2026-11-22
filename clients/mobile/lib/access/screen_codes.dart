/// Sunucudaki ekran kataloğu ile aynı kodlar (Identity `ScreenCatalog`, docs/s-plus-ekranlar.md).
abstract final class ScreenCodes {
  static const playerSearch = 'player.search';
  static const playerLocator = 'player.locator';
  static const loyaltyPoints = 'loyalty.points';
  static const loyaltyComp = 'loyalty.comp';
  static const liveTables = 'live.tables';
  static const liveRating = 'live.rating';
  static const slotFloor = 'slot.floor';
  static const reportDaily = 'report.daily';

  // Yetkisi olsa da mobilde ekranı olmayanlar (API ucu yok, bkz. specs/014-mobil/spec.md)
  static const playerHistory = 'player.history';
  static const hostPortfolio = 'loyalty.host_portfolio';
}

abstract final class ScreenActions {
  static const view = 'view';
  static const request = 'request';
  static const approve = 'approve';
}
