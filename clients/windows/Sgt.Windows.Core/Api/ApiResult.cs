using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Api;

/// <summary>
/// Sunucunun RFC 7807 hata yanıtı (problem details) veya ağ hatası. Status 0: sunucuya ulaşılamadı.
/// <see cref="Code"/> API'nin hata kodudur (ör. "Finance.VarianceNoteRequired").
/// </summary>
public sealed record ApiProblem(int Status, string? Code, string Title, string? Detail)
{
    /// <summary>Kullanıcıya gösterilecek metin: ayrıntı varsa ayrıntı, yoksa başlık.</summary>
    public string Message => string.IsNullOrWhiteSpace(Detail) ? Title : Detail;

    public static ApiProblem Unreachable() => new(0, null, Strings.Api_Unreachable, null);

    public static ApiProblem InvalidResponse() => new(0, null, Strings.Api_InvalidResponse, null);
}

/// <summary>Gövdesiz başarılı yanıt (204).</summary>
public readonly record struct Done;

/// <summary>201 Created gövdesi: { "id": ... }.</summary>
[ApiSchema("CreatedId")]
public sealed record CreatedDto(Guid Id);

/// <summary>API çağrısının sonucu: başarılıysa değer, değilse <see cref="ApiProblem"/>. İstisna fırlatılmaz.</summary>
public sealed class ApiResult<T>
{
    private readonly T? _value;

    internal ApiResult(T? value, ApiProblem? problem)
    {
        _value = value;
        Problem = problem;
    }

    public bool IsSuccess => Problem is null;

    public ApiProblem? Problem { get; }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Başarısız sonucun değeri yoktur.");
}

public static class ApiResult
{
    public static ApiResult<T> Success<T>(T value) => new(value, null);

    public static ApiResult<Done> Success() => new(default, null);

    public static ApiResult<T> Failure<T>(ApiProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return new ApiResult<T>(default, problem);
    }
}
