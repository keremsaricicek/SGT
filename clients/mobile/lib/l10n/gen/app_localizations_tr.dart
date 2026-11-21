// ignore: unused_import
import 'package:intl/intl.dart' as intl;

import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Turkish (`tr`).
class AppLocalizationsTr extends AppLocalizations {
  AppLocalizationsTr([String locale = 'tr']) : super(locale);

  @override
  String get appTitle => 'S Plus';

  @override
  String get appSubtitle => 'Mobil';

  @override
  String get loginButton => 'Giriş yap';

  @override
  String get loginHint =>
      'Giriş casinonun kimlik sunucusunda yapılır. Şifreniz ve doğrulama kodunuz bu uygulamaya girmez.';

  @override
  String get loginFailed => 'Giriş tamamlanamadı. Tekrar deneyin.';

  @override
  String get loginMissingClaims =>
      'Hesabınız bir tesise bağlı değil. Sistem yöneticinize başvurun.';

  @override
  String get signOut => 'Çıkış';

  @override
  String get menuEmpty => 'Size atanmış mobil ekran yok.';

  @override
  String get areaPlayer => 'Oyuncu';

  @override
  String get areaLoyalty => 'Loyalty / Host';

  @override
  String get areaLive => 'Canlı oyun';

  @override
  String get areaSlot => 'Slot';

  @override
  String get areaReports => 'Raporlar';

  @override
  String get screenPlayerSearch => 'Oyuncu arama';

  @override
  String get screenPlayerLocator => 'Oyuncu nerede';

  @override
  String get screenLoyaltyPoints => 'Puan ve seviye';

  @override
  String get screenCompRequest => 'Comp talebi';

  @override
  String get screenDailySummary => 'Günlük özet';

  @override
  String get screenTables => 'Masa haritası';

  @override
  String get screenSlots => 'Slot salonu';

  @override
  String get screenPlayer => 'Oyuncu';

  @override
  String get searchHint => 'Oyuncu no, ad veya kart no';

  @override
  String get searchMinChars => 'Aramak için en az 2 karakter yazın.';

  @override
  String get searchNoResults => 'Sonuç yok.';

  @override
  String resultCount(int count) {
    return '$count sonuç';
  }

  @override
  String get restricted => 'KISITLI';

  @override
  String get noCard => 'Kart yok';

  @override
  String get sectionLocation => 'Şu an nerede';

  @override
  String playingAt(String table) {
    return '$table masasında oynuyor';
  }

  @override
  String get since => 'Başlangıç';

  @override
  String get notPlaying => 'Şu an oyunda değil.';

  @override
  String get sectionLoyalty => 'Puan';

  @override
  String get balance => 'Bakiye';

  @override
  String get lifetimeEarned => 'Toplam kazanılan';

  @override
  String get tier => 'Seviye';

  @override
  String get recentEntries => 'Son hareketler';

  @override
  String get noEntries => 'Hareket yok.';

  @override
  String get kindEarned => 'Kazanım';

  @override
  String get kindRedeemed => 'Harcama';

  @override
  String get kindAdjusted => 'Düzeltme';

  @override
  String get requestComp => 'Comp talep et';

  @override
  String get fieldPoints => 'Puan';

  @override
  String get fieldText => 'Açıklama';

  @override
  String get fieldReason => 'Gerekçe kodu';

  @override
  String get fieldReasonHelp => 'Örn. COMP_YEMEK (en çok 50 karakter)';

  @override
  String get submitRequest => 'Talebi gönder';

  @override
  String get errPointsPositive => 'Pozitif tam sayı girin.';

  @override
  String get errRequired => 'Zorunlu alan.';

  @override
  String get errReasonLength => '1–50 karakter olmalı.';

  @override
  String get compSent => 'Talep gönderildi; onay bekliyor.';

  @override
  String get compRetryHint =>
      'Aynı bilgilerle tekrar gönderebilirsiniz; çift talep oluşmaz.';

  @override
  String get date => 'Tarih';

  @override
  String get today => 'Bugün';

  @override
  String get period => 'Dönem';

  @override
  String get sectionCage => 'Kasa';

  @override
  String get shiftsOpen => 'Açık vardiya';

  @override
  String get shiftsClosed => 'Kapanan vardiya';

  @override
  String get reversals => 'Ters kayıt';

  @override
  String get chipSales => 'Çip satışı';

  @override
  String get chipRedemptions => 'Çip bozdurma';

  @override
  String get closingVariance => 'Kapanış farkı';

  @override
  String get sectionLive => 'Canlı oyun';

  @override
  String get ratingsFinished => 'Biten rating';

  @override
  String get theoreticalWin => 'Teorik kazanç';

  @override
  String get casinoResult => 'Casino sonucu';

  @override
  String get sectionSlot => 'Slot';

  @override
  String get coinIn => 'Giren';

  @override
  String get machineWin => 'Kazanç';

  @override
  String get pointsEarned => 'Kazanılan puan';

  @override
  String get pointsRedeemed => 'Harcanan puan';

  @override
  String get currency => 'Döviz';

  @override
  String get noData => 'Veri yok.';

  @override
  String get tableOpen => 'AÇIK';

  @override
  String get tableClosed => 'KAPALI';

  @override
  String get limits => 'Limit';

  @override
  String get colTable => 'Masa';

  @override
  String get colGame => 'Oyun';

  @override
  String get colStatus => 'Durum';

  @override
  String get colRatings => 'Rating';

  @override
  String tablesSummary(int open, int total) {
    return '$open açık / $total masa';
  }

  @override
  String get colMachine => 'Makine';

  @override
  String get lastRead => 'Son okuma';

  @override
  String get neverRead => 'Okuma yok';

  @override
  String machineCount(int count) {
    return '$count makine';
  }

  @override
  String get errNetwork => 'Sunucuya ulaşılamadı. Bağlantıyı kontrol edin.';

  @override
  String get errForbidden => 'Bu işlem için yetkiniz yok.';

  @override
  String get errNotFound => 'Kayıt bulunamadı.';

  @override
  String get errConflict => 'İşlem şu an yapılamıyor.';

  @override
  String get errValidation => 'Girilen bilgiler geçersiz.';

  @override
  String get errServer => 'Sunucu hatası. Daha sonra tekrar deneyin.';

  @override
  String get errInvalidResponse => 'Sunucudan beklenmeyen yanıt geldi.';

  @override
  String get errSession => 'Oturum sona erdi. Tekrar giriş yapın.';

  @override
  String get retry => 'Tekrar dene';

  @override
  String get refresh => 'Yenile';

  @override
  String get noAccessTitle => 'Yetkiniz yok';

  @override
  String get noAccessBody =>
      'Bu ekran size atanmamış. Yetki için sistem yöneticinize başvurun.';

  @override
  String statusTerminal(String id) {
    return 'Terminal $id';
  }

  @override
  String get readOnly => 'Salt okunur';
}
