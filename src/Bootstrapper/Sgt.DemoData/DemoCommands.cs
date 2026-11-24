using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>Demo verisi bir modül kuralına takıldı; üretim durur (veri yarım kalırsa <c>demo.sh reset</c>).</summary>
public sealed class DemoSeedException(string message) : Exception(message);

/// <summary>
/// Modül handler'larını, bir HTTP isteğindeki gibi her çağrıda yeni bir DI kapsamında çalıştırır (Spec 012 R1).
/// Tablolara doğrudan yazılmaz; iş kuralları ve denetim kaydı handler'larda aynen işler (K2).
/// </summary>
public sealed class DemoCommands(IServiceScopeFactory scopes)
{
    /// <summary>Sonuç değeri olmayan komut; hata üretimi durdurur.</summary>
    public async Task Do<THandler>(string what, Func<THandler, Task<Result>> call)
        where THandler : notnull
    {
        var result = await Try(call);
        if (result.IsFailure)
        {
            throw Failure(what, result.Error);
        }
    }

    /// <summary>Sonuç değeri olan komut; hata üretimi durdurur.</summary>
    public async Task<T> Ask<THandler, T>(string what, Func<THandler, Task<Result<T>>> call)
        where THandler : notnull
    {
        ArgumentNullException.ThrowIfNull(call);
        await using var scope = scopes.CreateAsyncScope();
        var result = await call(scope.ServiceProvider.GetRequiredService<THandler>());
        return result.IsSuccess ? result.Value : throw Failure(what, result.Error);
    }

    /// <summary>Sonucu çağırana bırakan komut (ör. "zaten var" kabul edilebilir).</summary>
    public async Task<Result> Try<THandler>(Func<THandler, Task<Result>> call)
        where THandler : notnull
    {
        ArgumentNullException.ThrowIfNull(call);
        await using var scope = scopes.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<THandler>());
    }

    public async Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>().Handle(query, ct);
    }

    private static DemoSeedException Failure(string what, Error error) => new($"Demo verisi yazılamadı ({what}): {error.Code} {error.Message}");
}
