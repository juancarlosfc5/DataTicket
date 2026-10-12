using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Cliente HTTP que se comporta como el navegador del SPA: guarda y envía cookies (borra las que el servidor
/// expira), no sigue redirecciones y adjunta la cabecera antiforgery cuando se le pide. Gestiona las cookies
/// a mano para que las pruebas vean cada <c>Set-Cookie</c> y puedan trasladar una cookie a otro host.
/// </summary>
public sealed class TestBrowser : IDisposable
{
    public const string AuthCookie = "dataticket.auth";
    public const string AntiforgeryCookie = "dataticket.xsrf";
    public const string AntiforgeryHeader = "X-XSRF-TOKEN";

    public static readonly Uri Https = new("https://localhost");
    public static readonly Uri Http = new("http://localhost");

    private readonly HttpClient _client;
    private readonly TimeProvider _clock;
    private readonly Dictionary<string, string> _cookies = new(StringComparer.Ordinal);

    public TestBrowser(WebApplicationFactory<Program> factory, Uri? baseAddress = null)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
            BaseAddress = baseAddress ?? Https,
        });

        // El navegador comparte el reloj del host (FakeTimeProvider en las fixtures): una cookie expira
        // según el tiempo de la aplicación, no según el reloj real de la máquina.
        _clock = factory.Services.GetRequiredService<TimeProvider>();
    }

    public IReadOnlyDictionary<string, string> Cookies => _cookies;

    /// <summary>Último token antiforgery pedido con <see cref="FetchAntiforgeryTokenAsync"/>.</summary>
    public string? AntiforgeryToken { get; private set; }

    public void SetCookie(string name, string value) => _cookies[name] = value;

    public Task<HttpResponseMessage> GetAsync(string path, CancellationToken cancellationToken) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Get, path), cancellationToken);

    /// <summary>GET con una cabecera <c>Accept</c> concreta (p. ej. <c>text/html</c>, como una navegación).</summary>
    public Task<HttpResponseMessage> GetAsync(string path, string accept, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation(HeaderNames.Accept, accept);
        return SendAsync(request, cancellationToken);
    }

    public Task<HttpResponseMessage> PostJsonAsync(
        string path, object? body, string? antiforgeryToken, CancellationToken cancellationToken) =>
        PostAsync(path, body is null ? null : JsonContent.Create(body), antiforgeryToken, accept: null, cancellationToken);

    /// <summary>POST con el cuerpo tal cual (p. ej. JSON mal formado) y, si se pide, una cabecera <c>Accept</c>.</summary>
    public Task<HttpResponseMessage> PostAsync(
        string path, HttpContent? content, string? antiforgeryToken, string? accept, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        if (accept is not null)
        {
            request.Headers.TryAddWithoutValidation(HeaderNames.Accept, accept);
        }

        if (antiforgeryToken is not null)
        {
            request.Headers.Add(AntiforgeryHeader, antiforgeryToken);
        }

        return SendAsync(request, cancellationToken);
    }

    /// <summary>Pide un token antiforgery (como hace el SPA antes de cada mutación tras login/logout).</summary>
    public async Task<string> FetchAntiforgeryTokenAsync(CancellationToken cancellationToken)
    {
        using var response = await GetAsync("/api/auth/antiforgery", cancellationToken);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        AntiforgeryToken = json.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("La respuesta antiforgery no trae token.");
        return AntiforgeryToken;
    }

    /// <summary>Flujo completo del SPA: token antiforgery y <c>POST /api/auth/login</c>.</summary>
    public async Task<HttpResponseMessage> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var token = await FetchAntiforgeryTokenAsync(cancellationToken);
        return await PostJsonAsync("/api/auth/login", new { email, password }, token, cancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_cookies.Count > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", _cookies.Select(cookie => $"{cookie.Key}={cookie.Value}")));
        }

        var response = await _client.SendAsync(request, cancellationToken);
        foreach (var cookie in SetCookies(response))
        {
            var name = cookie.Name.ToString();
            var expired = cookie.Expires is { } expires && expires <= _clock.GetUtcNow();
            if (expired || cookie.Value.Length == 0)
            {
                _cookies.Remove(name);
            }
            else
            {
                _cookies[name] = cookie.Value.ToString();
            }
        }

        return response;
    }

    /// <summary>Cabeceras <c>Set-Cookie</c> de una respuesta, ya interpretadas.</summary>
    public static IReadOnlyList<SetCookieHeaderValue> SetCookies(HttpResponseMessage response) =>
        response.Headers.TryGetValues(HeaderNames.SetCookie, out var values)
            ? [.. SetCookieHeaderValue.ParseList([.. values])]
            : [];

    public static SetCookieHeaderValue? FindSetCookie(HttpResponseMessage response, string name) =>
        SetCookies(response).FirstOrDefault(cookie => cookie.Name.Equals(name, StringComparison.Ordinal));

    public void Dispose() => _client.Dispose();
}
