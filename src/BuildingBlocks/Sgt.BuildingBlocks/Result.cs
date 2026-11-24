namespace Sgt.BuildingBlocks;

/// <summary>
/// Bir işlemin sonucu: başarılı ya da bir <see cref="Error"/> ile başarısız.
/// Beklenen iş kuralı ihlalleri exception yerine bununla döner.
/// </summary>
public class Result
{
    protected Result(Error error)
    {
        Error = error;
    }

    public Error Error { get; }

    public bool IsSuccess => Error == Error.None;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(Error.None);

    public static Result Failure(Error error) => new(EnsureRealError(error));

    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);

    protected static Error EnsureRealError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error == Error.None)
        {
            throw new ArgumentException("Başarısız sonuç gerçek bir hata taşımalıdır.", nameof(error));
        }

        return error;
    }
}

/// <summary>Başarılıysa bir değer taşıyan sonuç.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value)
        : base(Error.None)
    {
        _value = value;
    }

    internal Result(Error error)
        : base(EnsureRealError(error))
    {
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Başarısız sonucun değeri okunamaz: {Error.Code}");

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}
