using System.Net;
using System.Text;
using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Auth;
using Sgt.Windows.Core.Features.Cage;

namespace Sgt.Windows.Core.Tests.Api;

public sealed class HttpPipelineTests : IDisposable
{
    private readonly RecordingHandler _server = new();
    private readonly CorrelationContext _correlation = new();
    private readonly ITokenProvider _tokens = Substitute.For<ITokenProvider>();

    public void Dispose() => _server.Dispose();

    private ApiTransport CreateTransport()
    {
        var terminal = Substitute.For<ITerminalInfo>();
        terminal.Name.Returns("KASA-1");
        var headers = new SgtHeadersHandler(terminal, _correlation, new SequentialIds()) { InnerHandler = _server };
        var bearer = new BearerTokenHandler(_tokens) { InnerHandler = headers };
        return new ApiTransport(new HttpClient(bearer) { BaseAddress = new Uri("https://api.test/") });
    }

    [Fact]
    public async Task Her_istek_terminal_correlation_ve_token_basliklarini_tasir()
    {
        _tokens.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("token-1");
        _server.Respond(HttpStatusCode.OK, "[]", "application/json");
        var transport = CreateTransport();

        await transport.GetAsync<List<object>>(ApiRoutes.MyMenu.Build(), CancellationToken.None);

        var request = Assert.Single(_server.Requests);
        Assert.Equal("https://api.test/api/v1/identity/me/menu", request.Uri);
        Assert.Equal("KASA-1", request.Headers["X-Terminal"]);
        Assert.True(Guid.TryParse(request.Headers["X-Correlation-Id"], out _));
        Assert.Equal("Bearer token-1", request.Headers["Authorization"]);
    }

    [Fact]
    public async Task Islem_kapsamindaki_istekler_ayni_correlation_id_tasir()
    {
        _server.Respond(HttpStatusCode.NoContent, null, null);
        var transport = CreateTransport();
        var id = Guid.Parse("55555555-5555-5555-5555-555555555555");

        using (_correlation.Begin(id))
        {
            await transport.PostAsync(ApiRoutes.CloseShift.Build(Guid.Empty), new { }, CancellationToken.None);
            await transport.PostAsync(ApiRoutes.CloseShift.Build(Guid.Empty), new { }, CancellationToken.None);
        }

        await transport.PostAsync(ApiRoutes.CloseShift.Build(Guid.Empty), new { }, CancellationToken.None);

        Assert.Equal(id.ToString("D"), _server.Requests[0].Headers["X-Correlation-Id"]);
        Assert.Equal(id.ToString("D"), _server.Requests[1].Headers["X-Correlation-Id"]);
        Assert.NotEqual(id.ToString("D"), _server.Requests[2].Headers["X-Correlation-Id"]);
        Assert.False(_server.Requests[0].Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task Problem_details_yaniti_ApiProblem_olur()
    {
        _server.Respond(
            HttpStatusCode.Conflict,
            """{"type":"x","title":"Finance.InsufficientChips","status":409,"detail":"Çekmecede yeterli çip yok.","code":"Finance.InsufficientChips"}""",
            "application/problem+json");
        var transport = CreateTransport();

        var result = await transport.PostAsync<CageTransactionDto>(ApiRoutes.ChipSales.Build(Guid.Empty), new { }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Problem!.Status);
        Assert.Equal("Finance.InsufficientChips", result.Problem.Code);
        Assert.Equal("Çekmecede yeterli çip yok.", result.Problem.Message);
    }

    [Fact]
    public async Task Govdesiz_hata_durum_koduna_gore_mesaj_alir()
    {
        _server.Respond(HttpStatusCode.Forbidden, null, null);
        var transport = CreateTransport();

        var result = await transport.GetAsync<object>(ApiRoutes.ListTables.Build(), CancellationToken.None);

        Assert.Equal(403, result.Problem!.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Problem.Message));
    }

    [Fact]
    public async Task Ag_hatasi_istisna_degil_ulasilamadi_problemi_olur()
    {
        _server.Fail(new HttpRequestException("bağlantı reddedildi"));
        var transport = CreateTransport();

        var result = await transport.GetAsync<object>(ApiRoutes.ListTables.Build(), CancellationToken.None);

        Assert.Equal(0, result.Problem!.Status);
    }

    [Fact]
    public async Task Basarili_yanit_json_olarak_okunur_ve_enumlar_metindir()
    {
        _server.Respond(HttpStatusCode.OK, """{"id":"66666666-6666-6666-6666-666666666666"}""", "application/json");
        var transport = CreateTransport();

        var result = await transport.PostAsync<CreatedDto>(
            ApiRoutes.OpenShift.Build(), new OpenShiftRequest([new DrawerAmount(Instrument.Chips, "TRY", 10.5m)], "SHIFT_OPEN"), CancellationToken.None);

        Assert.Equal(Guid.Parse("66666666-6666-6666-6666-666666666666"), result.Value.Id);
        Assert.Equal(
            """{"opening":[{"instrument":"Chips","currency":"TRY","amount":10.5}],"reasonCode":"SHIFT_OPEN"}""",
            _server.Requests[0].Body);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private HttpStatusCode _status = HttpStatusCode.OK;
        private string? _body;
        private string? _mediaType;
        private Exception? _failure;

        public List<RecordedRequest> Requests { get; } = [];

        public void Respond(HttpStatusCode status, string? body, string? mediaType)
        {
            _status = status;
            _body = body;
            _mediaType = mediaType;
        }

        public void Fail(Exception failure) => _failure = failure;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RecordedRequest(request.RequestUri!.ToString(), headers, body));
            if (_failure is not null)
            {
                throw _failure;
            }

            var response = new HttpResponseMessage(_status);
            if (_body is not null)
            {
                response.Content = new StringContent(_body, Encoding.UTF8, _mediaType!);
            }

            return response;
        }
    }

    private sealed record RecordedRequest(string Uri, Dictionary<string, string> Headers, string? Body);
}
