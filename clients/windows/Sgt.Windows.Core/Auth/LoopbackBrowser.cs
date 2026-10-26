using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Duende.IdentityModel.OidcClient.Browser;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Auth;

/// <summary>
/// RFC 8252 "loopback" yöntemi: giriş sayfası sistem tarayıcısında açılır, Keycloak sonucu
/// http://127.0.0.1:PORT/ adresine yönlendirir; bu sınıf o tek isteği dinleyip OIDC istemcisine verir.
/// </summary>
internal sealed class LoopbackBrowser(ISystemBrowser systemBrowser, int port) : IBrowser
{
    public string RedirectUri { get; } = $"http://127.0.0.1:{port}/";

    public static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        using var listener = new HttpListener();
        listener.Prefixes.Add(RedirectUri);
        listener.Start();

        systemBrowser.Open(new Uri(options.StartUrl));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.Timeout);
        try
        {
            var context = await listener.GetContextAsync().WaitAsync(timeout.Token);
            var response = context.Request.Url?.ToString() ?? string.Empty;
            await WriteDonePage(context.Response, timeout.Token);
            return new BrowserResult { ResultType = BrowserResultType.Success, Response = response };
        }
        catch (OperationCanceledException)
        {
            return new BrowserResult
            {
                ResultType = cancellationToken.IsCancellationRequested ? BrowserResultType.UserCancel : BrowserResultType.Timeout,
                Error = Strings.Login_Timeout,
            };
        }
    }

    private static async Task WriteDonePage(HttpListenerResponse response, CancellationToken ct)
    {
        var html = $"<!doctype html><html><head><meta charset=\"utf-8\"><title>SGT</title></head><body style=\"font-family:sans-serif\"><p>{WebUtility.HtmlEncode(Strings.Login_BrowserDone)}</p></body></html>";
        var bytes = Encoding.UTF8.GetBytes(html);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes, ct);
        response.OutputStream.Close();
    }
}

/// <summary>Adresi işletim sisteminin varsayılan tarayıcısında açar.</summary>
internal sealed class ShellSystemBrowser : ISystemBrowser
{
    public void Open(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        using var process = Process.Start(new ProcessStartInfo(url.AbsoluteUri) { UseShellExecute = true });
    }
}
