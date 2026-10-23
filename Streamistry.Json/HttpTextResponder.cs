using System.Diagnostics.CodeAnalysis;

namespace Streamistry.Json;

/// <summary>HTTP execution only. Compose URL mapping before it and JSON parsing afterward.</summary>
public sealed class HttpTextResponder(IChainablePort<Uri> upstream, HttpClient client, CancellationToken cancellationToken = default)
    : TryRouterPipe<Uri, string>(upstream)
{
    protected override bool TryInvoke(Uri input, [NotNullWhen(true)] out string? value)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, input);
        using var response = client.Send(request, cancellationToken);
        if (!response.IsSuccessStatusCode) { value = null; return false; }
        using var stream = response.Content.ReadAsStream(cancellationToken);
        using var reader = new StreamReader(stream);
        value = reader.ReadToEnd();
        cancellationToken.ThrowIfCancellationRequested();
        return true;
    }

    /// <summary>Awaitable HTTP helper for explicit async composition; borrows the client.</summary>
    public static async Task<string> ReadAsync(HttpClient client, Uri uri, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}
