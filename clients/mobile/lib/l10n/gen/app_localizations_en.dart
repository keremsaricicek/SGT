// ignore: unused_import
import 'package:intl/intl.dart' as intl;

import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'S Plus';

  @override
  String get appSubtitle => 'Mobile';

  @override
  String get loginButton => 'Sign in';

  @override
  String get loginHint =>
      'You sign in on the casino\'s identity server. Your password and verification code never enter this app.';

  @override
  String get loginFailed => 'Sign-in could not be completed. Try again.';

  @override
  String get loginMissingClaims =>
      'Your account is not linked to a property. Contact your system administrator.';

  @override
  String get signOut => 'Sign out';

  @override
  String get menuEmpty => 'No mobile screens are assigned to you.';

  @override
  String get areaPlayer => 'Player';

  @override
  String get areaLoyalty => 'Loyalty / Host';

  @override
  String get areaLive => 'Live games';

  @override
  String get areaSlot => 'Slots';

  @override
  String get areaReports => 'Reports';

  @override
  String get screenPlayerSearch => 'Player search';

  @override
  String get screenPlayerLocator => 'Where is the player';

  @override
  String get screenLoyaltyPoints => 'Points and tier';

  @override
  String get screenCompRequest => 'Comp request';

  @override
  String get screenDailySummary => 'Daily summary';

  @override
  String get screenTables => 'Table map';

  @override
  String get screenSlots => 'Slot floor';

  @override
  String get screenPlayer => 'Player';

  @override
  String get searchHint => 'Player no, name or card no';

  @override
  String get searchMinChars => 'Type at least 2 characters to search.';

  @override
  String get searchNoResults => 'No results.';

  @override
  String resultCount(int count) {
    return '$count results';
  }

  @override
  String get restricted => 'RESTRICTED';

  @override
  String get noCard => 'No card';

  @override
  String get sectionLocation => 'Where now';

  @override
  String playingAt(String table) {
    return 'Playing at table $table';
  }

  @override
  String get since => 'Since';

  @override
  String get notPlaying => 'Not playing right now.';

  @override
  String get sectionLoyalty => 'Points';

  @override
  String get balance => 'Balance';

  @override
  String get lifetimeEarned => 'Lifetime earned';

  @override
  String get tier => 'Tier';

  @override
  String get recentEntries => 'Recent entries';

  @override
  String get noEntries => 'No entries.';

  @override
  String get kindEarned => 'Earned';

  @override
  String get kindRedeemed => 'Redeemed';

  @override
  String get kindAdjusted => 'Adjustment';

  @override
  String get requestComp => 'Request comp';

  @override
  String get fieldPoints => 'Points';

  @override
  String get fieldText => 'Description';

  @override
  String get fieldReason => 'Reason code';

  @override
  String get fieldReasonHelp => 'E.g. COMP_DINNER (max 50 characters)';

  @override
  String get submitRequest => 'Send request';

  @override
  String get errPointsPositive => 'Enter a positive whole number.';

  @override
  String get errRequired => 'Required.';

  @override
  String get errReasonLength => 'Must be 1–50 characters.';

  @override
  String get compSent => 'Request sent; awaiting approval.';

  @override
  String get compRetryHint =>
      'You can send again with the same details; no duplicate request is created.';

  @override
  String get date => 'Date';

  @override
  String get today => 'Today';

  @override
  String get period => 'Period';

  @override
  String get sectionCage => 'Cage';

  @override
  String get shiftsOpen => 'Open shifts';

  @override
  String get shiftsClosed => 'Closed shifts';

  @override
  String get reversals => 'Reversals';

  @override
  String get chipSales => 'Chip sales';

  @override
  String get chipRedemptions => 'Chip redemptions';

  @override
  String get closingVariance => 'Closing variance';

  @override
  String get sectionLive => 'Live games';

  @override
  String get ratingsFinished => 'Finished ratings';

  @override
  String get theoreticalWin => 'Theoretical win';

  @override
  String get casinoResult => 'Casino result';

  @override
  String get sectionSlot => 'Slots';

  @override
  String get coinIn => 'Coin in';

  @override
  String get machineWin => 'Win';

  @override
  String get pointsEarned => 'Points earned';

  @override
  String get pointsRedeemed => 'Points redeemed';

  @override
  String get currency => 'Currency';

  @override
  String get noData => 'No data.';

  @override
  String get tableOpen => 'OPEN';

  @override
  String get tableClosed => 'CLOSED';

  @override
  String get limits => 'Limits';

  @override
  String get colTable => 'Table';

  @override
  String get colGame => 'Game';

  @override
  String get colStatus => 'Status';

  @override
  String get colRatings => 'Ratings';

  @override
  String tablesSummary(int open, int total) {
    return '$open open / $total tables';
  }

  @override
  String get colMachine => 'Machine';

  @override
  String get lastRead => 'Last read';

  @override
  String get neverRead => 'No reading';

  @override
  String machineCount(int count) {
    return '$count machines';
  }

  @override
  String get errNetwork => 'Cannot reach the server. Check your connection.';

  @override
  String get errForbidden => 'You are not authorized for this action.';

  @override
  String get errNotFound => 'Record not found.';

  @override
  String get errConflict => 'The action cannot be completed right now.';

  @override
  String get errValidation => 'The entered information is invalid.';

  @override
  String get errServer => 'Server error. Try again later.';

  @override
  String get errInvalidResponse => 'Unexpected response from the server.';

  @override
  String get errSession => 'Your session has ended. Sign in again.';

  @override
  String get retry => 'Retry';

  @override
  String get refresh => 'Refresh';

  @override
  String get noAccessTitle => 'No access';

  @override
  String get noAccessBody =>
      'This screen is not assigned to you. Contact your system administrator.';

  @override
  String statusTerminal(String id) {
    return 'Terminal $id';
  }

  @override
  String get readOnly => 'Read-only';
}
