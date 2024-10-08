﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Path;

namespace Streamistry.Json;
public abstract class BaseJsonPathPlucker<TJson, T> : Mapper<TJson, T> where TJson : JsonNode
{
    public BaseJsonPathPlucker(IChainablePort<TJson> upstream, string path)
        : this(path, upstream)
    { }

    protected BaseJsonPathPlucker(string path, IChainablePort<TJson>? upstream = null)
        : base((x) => GetValue(x, JsonPath.Parse(path)), upstream)
    { }

    protected static T GetValue(TJson? node, JsonPath path)
    {
        var matches = path.Evaluate(node).Matches;
        if (matches.Count==0)
            return default!;
        if (matches[0].Value.TryGetValue<T>(out var value))
            return value;
        return default!;
    }
}

public class PathPlucker<T> : BaseJsonPathPlucker<JsonObject, T> 
{
    public PathPlucker(string path)
        : this(path, null)
    { }

    public PathPlucker(IChainablePort<JsonObject> upstream, string path)
        : this(path, upstream)
    { }

    public PathPlucker(string path, IChainablePort<JsonObject>? upstream = null)
        : base(path, upstream)
    { }
}

public class PathArrayPlucker<T> : BaseJsonPathPlucker<JsonArray, T>
{
    public PathArrayPlucker(IChainablePort<JsonArray> upstream, string path)
        : base(upstream, path)
    { }
}
