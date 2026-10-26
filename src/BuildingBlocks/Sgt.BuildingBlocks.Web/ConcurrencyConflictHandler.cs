using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Sgt.BuildingBlocks.Web;

/// <summary>Eşzamanlı değişiklik çakışmasını 409 Conflict yanıtına çevirir.</summary>
public sealed class ConcurrencyConflictHandler : IExceptionHandler
{
    public static readonly Error Conflict =
        new("Common.ConcurrencyConflict", "Kayıt başka bir kullanıcı tarafından değiştirildi; sayfayı yenileyip tekrar deneyin.", ErrorKind.Conflict);

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        if (exception is not ConcurrencyConflictException)
        {
            return false;
        }

        await ResultHttpExtensions.Problem(Conflict).ExecuteAsync(httpContext);
        return true;
    }
}
