using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Streamistry.Observability;
using Streamistry.Pipes.Parsers;

namespace Streamistry.Json;
public class RestResponder<TInput, TOutput> : TryRouterPipe<TInput, TOutput>, IProcessablePipe<TInput?> where TOutput : JsonNode
{
    protected HttpClient Client { get; }
    protected Func<TInput?, string> UrlBuiler { get; }

    public RestResponder(IChainablePort<TInput> upstream, HttpClient client, Func<TInput?, string> urlBuilder)
        : base(upstream)
    {
        Client = client;
        UrlBuiler = urlBuilder;
    }

    [Meter]
    protected override bool TryInvoke(TInput? obj, [NotNullWhen(true)] out TOutput? value)
    {
        value = null;
        var url = UrlBuiler.Invoke(obj);
        var response = RemoteInvoke(url);
        if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var content = ConvertHttpContentToString(response.Content);
            value = (TOutput?)JsonNode.Parse(content ?? string.Empty);
            if (value is not null)
                return true;
        }
        return false;
    }

    [Meter]
    [Trace]
    protected virtual HttpResponseMessage RemoteInvoke(string url)
    {
        var response = Client.Send(new HttpRequestMessage(HttpMethod.Get, url));
        return response;
    }

    [Trace]
    protected virtual string ConvertHttpContentToString(HttpContent content)
    {
        using (var stream = content.ReadAsStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }



}
