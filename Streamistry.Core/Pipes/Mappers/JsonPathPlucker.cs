using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Path;

namespace Streamistry.Pipes.Mappers;
public abstract class BaseJsonPathPlucker<TJson, T> : Mapper<TJson, T> where TJson : JsonNode
{
    public BaseJsonPathPlucker(IChainablePipe<TJson> upstream, string path)
        : base(upstream, (x) => GetValue(x, JsonPath.Parse(path)))
    { }

    protected static T? GetValue(TJson? node, JsonPath path)
    {
        var matches = path.Evaluate(node).Matches;
        if (matches.Count==0)
            return default;
        if (matches[0].Value.TryGetValue<T>(out var value))
            return value;
        return default;
    }
}

public class JsonPathPlucker<T> : BaseJsonPathPlucker<JsonObject, T> 
{
    public JsonPathPlucker(IChainablePipe<JsonObject> upstream, string path)
        : base(upstream, path)
    { }
}

public class JsonPathArrayPlucker<T> : BaseJsonPathPlucker<JsonArray, T>
{
    public JsonPathArrayPlucker(IChainablePipe<JsonArray> upstream, string path)
        : base(upstream, path)
    { }
}
