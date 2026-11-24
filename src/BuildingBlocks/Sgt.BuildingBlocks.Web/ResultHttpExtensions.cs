using Microsoft.AspNetCore.Http;

namespace Sgt.BuildingBlocks.Web;

/// <summary>İş sonucunu HTTP yanıtına çevirir. Hata türü durum kodunu belirler.</summary>
public static class ResultHttpExtensions
{
    public static IResult ToHttp(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    public static IResult ToHttp<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);
    }

    public static IResult Problem(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        var status = error.Kind switch
        {
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
