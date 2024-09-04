using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using Json.Path;


namespace Streamistry.Json;
public class ObjectPropertyAppender<TInputMain, TInputSecondary> : Zipper<TInputMain, TInputSecondary, TInputMain>
    where TInputMain : JsonNode
    where TInputSecondary : JsonNode
{

    public ObjectPropertyAppender(IChainablePort<TInputMain> mainUpstream, IChainablePort<TInputSecondary> secondUpstream, string path)
        : base(mainUpstream, secondUpstream, (x, y) => AppendProperty(x, y, path))
    { }

    private static TInputMain? AppendProperty(TInputMain? main, TInputSecondary? secondary, string path)
    {
        if (main is null)
            return null;
        if (secondary is null)
            return main;
        var segments = path.Split('.');
        AddOrReplaceProperty(main, string.Join('.',segments.Take(segments.Length - 1)), path.Split('.').Last(), secondary);
        return main;
    }

    private static void AddOrReplaceProperty(JsonNode root, string jsonPath, string propertyName, JsonNode value)
    {
        var path = JsonPath.Parse(jsonPath);
        var result = path.Evaluate(root);

        if (result.Matches != null && result.Matches.Count > 0)
        {
            foreach (var match in result.Matches)
            {
                if (match.Value is JsonObject jsonObject)
                    jsonObject[propertyName] = value;
            }
        }
        else
        {
            CreatePathAndAddProperty(root, jsonPath, propertyName, value);
        }
    }

    private static void CreatePathAndAddProperty(JsonNode root, string jsonPath, string propertyName, JsonNode value)
    {
        var segments = jsonPath.TrimStart('$').Split('.', StringSplitOptions.RemoveEmptyEntries);
        JsonNode current = root;

        foreach (var segment in segments)
        {
            if (current is JsonObject obj)
            {
                if (!obj.ContainsKey(segment))
                {
                    var newObject = new JsonObject();
                    obj[segment] = newObject;
                    current = newObject;
                }
                else
                {
                    current = obj[segment]!;
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid path or JSON structure.");
            }
        }

        if (current is JsonObject finalObj)
        {
            finalObj[propertyName] = value;
        }
        else
        {
            throw new InvalidOperationException("Final path segment is not an object.");
        }
    }
}
