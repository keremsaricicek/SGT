import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:material_ui/material_ui.dart';

import '../../../app/services.dart';
import '../../../auth/auth_service.dart';
import '../../../theme/sgt_theme.dart';
import '../../../theme/sgt_tokens.dart';
import '../../../ui/sgt_widgets.dart';

/// Giriş (Y1, KK1). Şifre alanı yoktur; Keycloak sayfası sistem tarayıcısında açılır.
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key, this.initialError});

  /// Golden/test için başlangıç hata mesajı.
  final String? initialError;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _error = widget.initialError;
  }

  Future<void> _signIn() async {
    final l = context.l10n;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await AppServices.of(context).auth.signIn();
    } on AuthException catch (e) {
      _error = e.reason == 'missing_claims' ? l.loginMissingClaims : l.loginFailed;
    } on Object {
      _error = l.loginFailed; // ayrıntı (token, kullanıcı) loglanmaz
    }
    if (mounted) setState(() => _busy = false);
  }

  @override
  Widget build(BuildContext context) {
    final p = context.palette;
    final l = context.l10n;
    final t = Theme.of(context).textTheme;
    return Scaffold(
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Container(
            color: p.header,
            child: SafeArea(
              bottom: false,
              child: Padding(
                padding: const EdgeInsets.fromLTRB(SgtTokens.space4, SgtTokens.space4, SgtTokens.space4, SgtTokens.space4),
                child: Row(
                  children: [
                    Container(
                      width: 36,
                      height: 36,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: p.primary,
                        borderRadius: const BorderRadius.all(Radius.circular(SgtTokens.radius)),
                      ),
                      child: Text('SGT', style: TextStyle(color: p.onPrimary, fontWeight: FontWeight.w600, fontSize: 12)),
                    ),
                    const SizedBox(width: SgtTokens.space3),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(l.appTitle, style: TextStyle(color: p.onHeader, fontSize: SgtTokens.fontTitle, fontWeight: FontWeight.w600)),
                        Text(l.appSubtitle, style: TextStyle(color: p.onHeader.withValues(alpha: 0.7), fontSize: SgtTokens.fontCaption)),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
          Expanded(
            child: ListView(
              padding: const EdgeInsets.all(SgtTokens.space4),
              children: [
                const SizedBox(height: SgtTokens.space4),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Icon(FluentIcons.shield_lock_24_regular, color: p.textMuted),
                    const SizedBox(width: SgtTokens.space2),
                    Expanded(child: Text(l.loginHint, style: t.bodyMedium!.copyWith(color: p.textMuted))),
                  ],
                ),
                const SizedBox(height: SgtTokens.space4),
                FilledButton.icon(
                  onPressed: _busy ? null : _signIn,
                  icon: const Icon(FluentIcons.person_24_regular, size: 18),
                  label: Text(l.loginButton),
                ),
                if (_busy) const Padding(padding: EdgeInsets.only(top: SgtTokens.space2), child: LinearProgressIndicator(minHeight: 2)),
                if (_error != null) ...[
                  const SizedBox(height: SgtTokens.space2),
                  MessagePanel(icon: FluentIcons.error_circle_24_regular, text: _error!, tone: TagTone.negative),
                ],
              ],
            ),
          ),
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.all(SgtTokens.space3),
              child: Text('Sentez Gaming Technologies', textAlign: TextAlign.center, style: t.bodySmall),
            ),
          ),
        ],
      ),
    );
  }
}
