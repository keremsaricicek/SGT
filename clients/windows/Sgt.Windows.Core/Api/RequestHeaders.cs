using System.Net.Http.Headers;
using Sgt.Windows.Core.Auth;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Api;

/// <summary>Bu cihazın terminal adı (X-Terminal). Audit kaydında işlemin hangi bilgisayardan yapıldığını gösterir.</summary>
public interface ITerminalInfo
{
    string Name { get; }
}

internal sealed class MachineTerminalInfo : ITerminalInfo
{
    public string Name { get; } = Environment.MachineName;
}

/// <summary>
/// Kullanıcı işlemi başına CorrelationId. Bir işlem birden çok istek yapıyor veya tekrar deneniyorsa
/// <see cref="Begin"/> kapsamındaki tüm istekler aynı kimliği taşır; kapsam yoksa her istek yeni kimlik alır.
/// </summary>
public interface ICorrelationContext
{
    Guid? Current { get; }

    IDisposable Begin(Guid correlationId);
}

internal sealed class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<Guid?> Value = new();

    public Guid? Current => Value.Value;

    public IDisposable Begin(Guid correlationId)
    {
        var previous = Value.Value;
        Value.Value = correlationId;
        return new Scope(previous);
    }

    private sealed class Scope(Guid? previous) : IDisposable
    {
        public void Dispose() => Value.Value = previous;
    }
}

/// <summary>Her isteğe X-Terminal ve X-Correlation-Id başlıklarını ekler (ADR-0003).</summary>
internal sealed class SgtHeadersHandler(ITerminalInfo terminal, ICorrelationContext correlation, IIdGenerator ids) : DelegatingHandler
{
    public const string TerminalHeader = "X-Terminal";
    public const string CorrelationHeader = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Headers.Remove(TerminalHeader);
        request.Headers.Remove(CorrelationHeader);
        request.Headers.Add(TerminalHeader, terminal.Name);
        request.Headers.Add(CorrelationHeader, (correlation.Current ?? ids.NewId()).ToString("D"));
        return base.SendAsync(request, cancellationToken);
    }
}

/// <summary>Oturum açıksa isteğe "Authorization: Bearer" ekler. Token yalnızca bellekte tutulur.</summary>
internal sealed class BearerTokenHandler(ITokenProvider tokens) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = await tokens.GetAccessTokenAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
