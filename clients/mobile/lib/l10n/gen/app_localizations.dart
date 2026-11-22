import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_en.dart';
import 'app_localizations_tr.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'gen/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('tr'),
    Locale('en'),
  ];

  /// No description provided for @appTitle.
  ///
  /// In tr, this message translates to:
  /// **'S Plus'**
  String get appTitle;

  /// No description provided for @appSubtitle.
  ///
  /// In tr, this message translates to:
  /// **'Mobil'**
  String get appSubtitle;

  /// No description provided for @loginButton.
  ///
  /// In tr, this message translates to:
  /// **'Giriş yap'**
  String get loginButton;

  /// No description provided for @loginHint.
  ///
  /// In tr, this message translates to:
  /// **'Giriş casinonun kimlik sunucusunda yapılır. Şifreniz ve doğrulama kodunuz bu uygulamaya girmez.'**
  String get loginHint;

  /// No description provided for @loginFailed.
  ///
  /// In tr, this message translates to:
  /// **'Giriş tamamlanamadı. Tekrar deneyin.'**
  String get loginFailed;

  /// No description provided for @loginMissingClaims.
  ///
  /// In tr, this message translates to:
  /// **'Hesabınız bir tesise bağlı değil. Sistem yöneticinize başvurun.'**
  String get loginMissingClaims;

  /// No description provided for @signOut.
  ///
  /// In tr, this message translates to:
  /// **'Çıkış'**
  String get signOut;

  /// No description provided for @menuEmpty.
  ///
  /// In tr, this message translates to:
  /// **'Size atanmış mobil ekran yok.'**
  String get menuEmpty;

  /// No description provided for @areaPlayer.
  ///
  /// In tr, this message translates to:
  /// **'Oyuncu'**
  String get areaPlayer;

  /// No description provided for @areaLoyalty.
  ///
  /// In tr, this message translates to:
  /// **'Loyalty / Host'**
  String get areaLoyalty;

  /// No description provided for @areaLive.
  ///
  /// In tr, this message translates to:
  /// **'Canlı oyun'**
  String get areaLive;

  /// No description provided for @areaSlot.
  ///
  /// In tr, this message translates to:
  /// **'Slot'**
  String get areaSlot;

  /// No description provided for @areaReports.
  ///
  /// In tr, this message translates to:
  /// **'Raporlar'**
  String get areaReports;

  /// No description provided for @screenPlayerSearch.
  ///
  /// In tr, this message translates to:
  /// **'Oyuncu arama'**
  String get screenPlayerSearch;

  /// No description provided for @screenPlayerLocator.
  ///
  /// In tr, this message translates to:
  /// **'Oyuncu nerede'**
  String get screenPlayerLocator;

  /// No description provided for @screenLoyaltyPoints.
  ///
  /// In tr, this message translates to:
  /// **'Puan ve seviye'**
  String get screenLoyaltyPoints;

  /// No description provided for @screenCompRequest.
  ///
  /// In tr, this message translates to:
  /// **'Comp talebi'**
  String get screenCompRequest;

  /// No description provided for @screenDailySummary.
  ///
  /// In tr, this message translates to:
  /// **'Günlük özet'**
  String get screenDailySummary;

  /// No description provided for @screenTables.
  ///
  /// In tr, this message translates to:
  /// **'Masa haritası'**
  String get screenTables;

  /// No description provided for @screenSlots.
  ///
  /// In tr, this message translates to:
  /// **'Slot salonu'**
  String get screenSlots;

  /// No description provided for @screenPlayer.
  ///
  /// In tr, this message translates to:
  /// **'Oyuncu'**
  String get screenPlayer;

  /// No description provided for @searchHint.
  ///
  /// In tr, this message translates to:
  /// **'Oyuncu no, ad veya kart no'**
  String get searchHint;

  /// No description provided for @searchMinChars.
  ///
  /// In tr, this message translates to:
  /// **'Aramak için en az 2 karakter yazın.'**
  String get searchMinChars;

  /// No description provided for @searchNoResults.
  ///
  /// In tr, this message translates to:
  /// **'Sonuç yok.'**
  String get searchNoResults;

  /// No description provided for @resultCount.
  ///
  /// In tr, this message translates to:
  /// **'{count} sonuç'**
  String resultCount(int count);

  /// No description provided for @restricted.
  ///
  /// In tr, this message translates to:
  /// **'KISITLI'**
  String get restricted;

  /// No description provided for @noCard.
  ///
  /// In tr, this message translates to:
  /// **'Kart yok'**
  String get noCard;

  /// No description provided for @sectionLocation.
  ///
  /// In tr, this message translates to:
  /// **'Şu an nerede'**
  String get sectionLocation;

  /// No description provided for @playingAt.
  ///
  /// In tr, this message translates to:
  /// **'{table} masasında oynuyor'**
  String playingAt(String table);

  /// No description provided for @since.
  ///
  /// In tr, this message translates to:
  /// **'Başlangıç'**
  String get since;

  /// No description provided for @notPlaying.
  ///
  /// In tr, this message translates to:
  /// **'Şu an oyunda değil.'**
  String get notPlaying;

  /// No description provided for @sectionLoyalty.
  ///
  /// In tr, this message translates to:
  /// **'Puan'**
  String get sectionLoyalty;

  /// No description provided for @balance.
  ///
  /// In tr, this message translates to:
  /// **'Bakiye'**
  String get balance;

  /// No description provided for @lifetimeEarned.
  ///
  /// In tr, this message translates to:
  /// **'Toplam kazanılan'**
  String get lifetimeEarned;

  /// No description provided for @tier.
  ///
  /// In tr, this message translates to:
  /// **'Seviye'**
  String get tier;

  /// No description provided for @recentEntries.
  ///
  /// In tr, this message translates to:
  /// **'Son hareketler'**
  String get recentEntries;

  /// No description provided for @noEntries.
  ///
  /// In tr, this message translates to:
  /// **'Hareket yok.'**
  String get noEntries;

  /// No description provided for @kindEarned.
  ///
  /// In tr, this message translates to:
  /// **'Kazanım'**
  String get kindEarned;

  /// No description provided for @kindRedeemed.
  ///
  /// In tr, this message translates to:
  /// **'Harcama'**
  String get kindRedeemed;

  /// No description provided for @kindAdjusted.
  ///
  /// In tr, this message translates to:
  /// **'Düzeltme'**
  String get kindAdjusted;

  /// No description provided for @requestComp.
  ///
  /// In tr, this message translates to:
  /// **'Comp talep et'**
  String get requestComp;

  /// No description provided for @fieldPoints.
  ///
  /// In tr, this message translates to:
  /// **'Puan'**
  String get fieldPoints;

  /// No description provided for @fieldText.
  ///
  /// In tr, this message translates to:
  /// **'Açıklama'**
  String get fieldText;

  /// No description provided for @fieldReason.
  ///
  /// In tr, this message translates to:
  /// **'Gerekçe kodu'**
  String get fieldReason;

  /// No description provided for @fieldReasonHelp.
  ///
  /// In tr, this message translates to:
  /// **'Örn. COMP_YEMEK (en çok 50 karakter)'**
  String get fieldReasonHelp;

  /// No description provided for @submitRequest.
  ///
  /// In tr, this message translates to:
  /// **'Talebi gönder'**
  String get submitRequest;

  /// No description provided for @errPointsPositive.
  ///
  /// In tr, this message translates to:
  /// **'Pozitif tam sayı girin.'**
  String get errPointsPositive;

  /// No description provided for @errRequired.
  ///
  /// In tr, this message translates to:
  /// **'Zorunlu alan.'**
  String get errRequired;

  /// No description provided for @errReasonLength.
  ///
  /// In tr, this message translates to:
  /// **'1–50 karakter olmalı.'**
  String get errReasonLength;

  /// No description provided for @compSent.
  ///
  /// In tr, this message translates to:
  /// **'Talep gönderildi; onay bekliyor.'**
  String get compSent;

  /// No description provided for @compRetryHint.
  ///
  /// In tr, this message translates to:
  /// **'Aynı bilgilerle tekrar gönderebilirsiniz; çift talep oluşmaz.'**
  String get compRetryHint;

  /// No description provided for @date.
  ///
  /// In tr, this message translates to:
  /// **'Tarih'**
  String get date;

  /// No description provided for @today.
  ///
  /// In tr, this message translates to:
  /// **'Bugün'**
  String get today;

  /// No description provided for @period.
  ///
  /// In tr, this message translates to:
  /// **'Dönem'**
  String get period;

  /// No description provided for @sectionCage.
  ///
  /// In tr, this message translates to:
  /// **'Kasa'**
  String get sectionCage;

  /// No description provided for @shiftsOpen.
  ///
  /// In tr, this message translates to:
  /// **'Açık vardiya'**
  String get shiftsOpen;

  /// No description provided for @shiftsClosed.
  ///
  /// In tr, this message translates to:
  /// **'Kapanan vardiya'**
  String get shiftsClosed;

  /// No description provided for @reversals.
  ///
  /// In tr, this message translates to:
  /// **'Ters kayıt'**
  String get reversals;

  /// No description provided for @chipSales.
  ///
  /// In tr, this message translates to:
  /// **'Çip satışı'**
  String get chipSales;

  /// No description provided for @chipRedemptions.
  ///
  /// In tr, this message translates to:
  /// **'Çip bozdurma'**
  String get chipRedemptions;

  /// No description provided for @closingVariance.
  ///
  /// In tr, this message translates to:
  /// **'Kapanış farkı'**
  String get closingVariance;

  /// No description provided for @sectionLive.
  ///
  /// In tr, this message translates to:
  /// **'Canlı oyun'**
  String get sectionLive;

  /// No description provided for @ratingsFinished.
  ///
  /// In tr, this message translates to:
  /// **'Biten rating'**
  String get ratingsFinished;

  /// No description provided for @theoreticalWin.
  ///
  /// In tr, this message translates to:
  /// **'Teorik kazanç'**
  String get theoreticalWin;

  /// No description provided for @casinoResult.
  ///
  /// In tr, this message translates to:
  /// **'Casino sonucu'**
  String get casinoResult;

  /// No description provided for @sectionSlot.
  ///
  /// In tr, this message translates to:
  /// **'Slot'**
  String get sectionSlot;

  /// No description provided for @coinIn.
  ///
  /// In tr, this message translates to:
  /// **'Giren'**
  String get coinIn;

  /// No description provided for @machineWin.
  ///
  /// In tr, this message translates to:
  /// **'Kazanç'**
  String get machineWin;

  /// No description provided for @pointsEarned.
  ///
  /// In tr, this message translates to:
  /// **'Kazanılan puan'**
  String get pointsEarned;

  /// No description provided for @pointsRedeemed.
  ///
  /// In tr, this message translates to:
  /// **'Harcanan puan'**
  String get pointsRedeemed;

  /// No description provided for @currency.
  ///
  /// In tr, this message translates to:
  /// **'Döviz'**
  String get currency;

  /// No description provided for @noData.
  ///
  /// In tr, this message translates to:
  /// **'Veri yok.'**
  String get noData;

  /// No description provided for @tableOpen.
  ///
  /// In tr, this message translates to:
  /// **'AÇIK'**
  String get tableOpen;

  /// No description provided for @tableClosed.
  ///
  /// In tr, this message translates to:
  /// **'KAPALI'**
  String get tableClosed;

  /// No description provided for @limits.
  ///
  /// In tr, this message translates to:
  /// **'Limit'**
  String get limits;

  /// No description provided for @colTable.
  ///
  /// In tr, this message translates to:
  /// **'Masa'**
  String get colTable;

  /// No description provided for @colGame.
  ///
  /// In tr, this message translates to:
  /// **'Oyun'**
  String get colGame;

  /// No description provided for @colStatus.
  ///
  /// In tr, this message translates to:
  /// **'Durum'**
  String get colStatus;

  /// No description provided for @colKind.
  ///
  /// In tr, this message translates to:
  /// **'Tür'**
  String get colKind;

  /// No description provided for @colRatings.
  ///
  /// In tr, this message translates to:
  /// **'Rating'**
  String get colRatings;

  /// No description provided for @tablesSummary.
  ///
  /// In tr, this message translates to:
  /// **'{open} açık / {total} masa'**
  String tablesSummary(int open, int total);

  /// No description provided for @colMachine.
  ///
  /// In tr, this message translates to:
  /// **'Makine'**
  String get colMachine;

  /// No description provided for @lastRead.
  ///
  /// In tr, this message translates to:
  /// **'Son okuma'**
  String get lastRead;

  /// No description provided for @neverRead.
  ///
  /// In tr, this message translates to:
  /// **'Okuma yok'**
  String get neverRead;

  /// No description provided for @machineCount.
  ///
  /// In tr, this message translates to:
  /// **'{count} makine'**
  String machineCount(int count);

  /// No description provided for @errNetwork.
  ///
  /// In tr, this message translates to:
  /// **'Sunucuya ulaşılamadı. Bağlantıyı kontrol edin.'**
  String get errNetwork;

  /// No description provided for @errForbidden.
  ///
  /// In tr, this message translates to:
  /// **'Bu işlem için yetkiniz yok.'**
  String get errForbidden;

  /// No description provided for @errNotFound.
  ///
  /// In tr, this message translates to:
  /// **'Kayıt bulunamadı.'**
  String get errNotFound;

  /// No description provided for @errConflict.
  ///
  /// In tr, this message translates to:
  /// **'İşlem şu an yapılamıyor.'**
  String get errConflict;

  /// No description provided for @errValidation.
  ///
  /// In tr, this message translates to:
  /// **'Girilen bilgiler geçersiz.'**
  String get errValidation;

  /// No description provided for @errServer.
  ///
  /// In tr, this message translates to:
  /// **'Sunucu hatası. Daha sonra tekrar deneyin.'**
  String get errServer;

  /// No description provided for @errInvalidResponse.
  ///
  /// In tr, this message translates to:
  /// **'Sunucudan beklenmeyen yanıt geldi.'**
  String get errInvalidResponse;

  /// No description provided for @errSession.
  ///
  /// In tr, this message translates to:
  /// **'Oturum sona erdi. Tekrar giriş yapın.'**
  String get errSession;

  /// No description provided for @retry.
  ///
  /// In tr, this message translates to:
  /// **'Tekrar dene'**
  String get retry;

  /// No description provided for @refresh.
  ///
  /// In tr, this message translates to:
  /// **'Yenile'**
  String get refresh;

  /// No description provided for @noAccessTitle.
  ///
  /// In tr, this message translates to:
  /// **'Yetkiniz yok'**
  String get noAccessTitle;

  /// No description provided for @noAccessBody.
  ///
  /// In tr, this message translates to:
  /// **'Bu ekran size atanmamış. Yetki için sistem yöneticinize başvurun.'**
  String get noAccessBody;

  /// No description provided for @statusTerminal.
  ///
  /// In tr, this message translates to:
  /// **'Terminal {id}'**
  String statusTerminal(String id);

  /// No description provided for @readOnly.
  ///
  /// In tr, this message translates to:
  /// **'Salt okunur'**
  String get readOnly;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['en', 'tr'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'en':
      return AppLocalizationsEn();
    case 'tr':
      return AppLocalizationsTr();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
