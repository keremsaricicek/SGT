import '../api/api_exception.dart';

/// Bir ekran verisinin durumu.
sealed class LoadState<T> {
  const LoadState();
}

final class Idle<T> extends LoadState<T> {
  const Idle();
}

final class Loading<T> extends LoadState<T> {
  const Loading();
}

final class Loaded<T> extends LoadState<T> {
  const Loaded(this.value);

  final T value;
}

final class Failed<T> extends LoadState<T> {
  const Failed(this.error);

  final ApiException error;
}

/// Çağrıyı çalıştırır; [ApiException] dışındaki hataları "beklenmeyen yanıt" sayar (ayrıntı loglanmaz).
Future<LoadState<T>> runLoad<T>(Future<T> Function() call) async {
  try {
    return Loaded(await call());
  } on ApiException catch (e) {
    return Failed(e);
  }
}
