import 'package:flutter_test/flutter_test.dart';
import 'package:sgt_mobile/api/api_exception.dart';
import 'package:sgt_mobile/shared/load_state.dart';

void main() {
  test('başarılı çağrı Loaded döner', () async {
    expect(await runLoad(() async => 5), isA<Loaded<int>>().having((s) => s.value, 'value', 5));
  });

  test('ApiException olduğu gibi Failed olur', () async {
    final r = await runLoad<int>(() async => throw const ApiException(ApiFailure.forbidden));
    expect((r as Failed).error.kind, ApiFailure.forbidden);
  });

  test('beklenmeyen hata Failed(invalidResponse) olur, ekran kırılmaz', () async {
    final r = await runLoad<int>(() async => throw StateError('beklenmedik'));
    expect((r as Failed).error.kind, ApiFailure.invalidResponse);
  });
}
