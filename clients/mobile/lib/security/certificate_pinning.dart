import 'dart:io';

import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';

/// Sertifika sabitleme kancası (Spec 014 KK17, MASVS-NETWORK).
///
/// Üretim kurulumunda casinonun API sertifikası (veya iç CA'sı) PEM olarak uygulamaya verilir
/// (`--dart-define=SGT_PINNED_CERT=assets/security/api.pem`). Verilirse sistem kök sertifikalarına
/// güvenilmez; yalnızca bu sertifikayla imzalanan sunucuya bağlanılır.
abstract final class PinnedHttpClientFactory {
  static http.Client create(List<int> pemBytes) {
    final context = SecurityContext(withTrustedRoots: false)..setTrustedCertificatesBytes(pemBytes);
    final client = HttpClient(context: context)..badCertificateCallback = (_, _, _) => false;
    return IOClient(client);
  }
}
