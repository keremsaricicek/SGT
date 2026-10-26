using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Api;

/// <summary>HTTP + JSON ayrıntısını özellik istemcilerinden saklar. Hata yanıtlarını <see cref="ApiProblem"/>'a çevirir.</summary>
public interface IApiTransport
{
    Task<ApiResult<T>> GetAsync<T>(Uri path, CancellationToken ct);

    /// <summary>Kayıt yoksa sunucu 204 döner; bu durumda başarılı sonuç ve null değer.</summary>
    Task<ApiResult<T?>> GetOptionalAsync<T>(Uri path, CancellationToken ct)
        where T : class;

    Task<ApiResult<T>> PostAsync<T>(Uri path, object body, CancellationToken ct);

    Task<ApiResult<Done>> PostAsync(Uri path, object body, CancellationToken ct);
}

internal sealed class ApiTransport(HttpClient http) : IApiTransport
{
    internal static readonly JsonSerializerOptions Json = CreateJsonOptions();

    public Task<ApiResult<T>> GetAsync<T>(Uri path, CancellationToken ct) =>
        Send(() => http.GetAsync(path, ct), ReadBody<T>, ct);

    public Task<ApiResult<T?>> GetOptionalAsync<T>(Uri path, CancellationToken ct)
        where T : class =>
        Send(
            () => http.GetAsync(path, ct),
            async (response, token) => response.StatusCode == HttpStatusCode.NoContent ? null : await ReadBody<T>(response, token),
            ct);

    public Task<ApiResult<T>> PostAsync<T>(Uri path, object body, CancellationToken ct) =>
        Send(() => http.PostAsJsonAsync(path, body, Json, ct), ReadBody<T>, ct);

    public Task<ApiResult<Done>> PostAsync(Uri path, object body, CancellationToken ct) =>
        Send(() => http.PostAsJsonAsync(path, body, Json, ct), (_, _) => Task.FromResult(default(Done)), ct);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static async Task<ApiResult<T>> Send<T>(
        Func<Task<HttpResponseMessage>> send, Func<HttpResponseMessage, CancellationToken, Task<T>> read, CancellationToken ct)
    {
        try
        {
            using var response = await send();
            return response.IsSuccessStatusCode
                ? ApiResult.Success(await read(response, ct))
                : ApiResult.Failure<T>(await ReadProblem(response, ct));
        }
        catch (HttpRequestException)
        {
            return ApiResult.Failure<T>(ApiProblem.Unreachable());
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return ApiResult.Failure<T>(ApiProblem.Unreachable());
        }
        catch (JsonException)
        {
            return ApiResult.Failure<T>(ApiProblem.InvalidResponse());
        }
    }

    private static async Task<T> ReadBody<T>(HttpResponseMessage response, CancellationToken ct) =>
        await response.Content.ReadFromJsonAsync<T>(Json, ct) ?? throw new JsonException("Boş yanıt.");

    private static async Task<ApiProblem> ReadProblem(HttpResponseMessage response, CancellationToken ct)
    {
        var status = (int)response.StatusCode;
        ProblemDocument? document = null;
        if (response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                document = await response.Content.ReadFromJsonAsync<ProblemDocument>(Json, ct);
            }
            catch (JsonException)
            {
                document = null;
            }
        }

        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Strings.Api_Unauthorized,
            HttpStatusCode.Forbidden => Strings.Api_Forbidden,
            HttpStatusCode.NotFound => Strings.Api_NotFound,
            _ => string.Format(System.Globalization.CultureInfo.CurrentCulture, Strings.Api_Failed, status),
        };

        return new ApiProblem(
            status,
            document?.Code,
            string.IsNullOrWhiteSpace(document?.Title) ? fallback : document.Title,
            document?.Detail);
    }

    /// <summary>RFC 7807 gövdesi; "code" alanını API ekler.</summary>
    private sealed record ProblemDocument(string? Title, string? Detail, int? Status, string? Code);
}
