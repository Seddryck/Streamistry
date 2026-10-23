using System.Text.Json;
using System.Text.Json.Nodes;
using global::Expressif;
using global::Expressif.Values;

namespace Streamistry.Json.Expressif;

public static class JsonRepresentation
{
    public static object? FromJson(JsonNode? node)
    {
        if (node is null) return null;
        if (node is JsonObject obj)
        {
            var record = new RecordValue();
            foreach (var field in obj) record.Set(field.Key, FromJson(field.Value));
            return record;
        }
        if (node is JsonArray array) return array.Select(FromJson).ToArray();
        using var document = JsonDocument.Parse(node.ToJsonString());
        var element = document.RootElement;
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new JsonException("Unsupported JSON scalar.")
        };
    }

    public static JsonNode? ToJson(object? value) => JsonSerializer.SerializeToNode(value);
}

public sealed class JsonExpressionMapper : Mapper<JsonNode?, JsonNode?>
{
    public JsonExpressionMapper(IChainablePort<JsonNode?> upstream, string definition, EvaluationContext? context = null)
        : this(upstream, Expression.Create(definition).WithContext(context ?? EvaluationContext.Empty)) { }
    public JsonExpressionMapper(IChainablePort<JsonNode?> upstream, IExpression expression)
        : base(upstream, node => JsonRepresentation.ToJson(expression.Evaluate(JsonRepresentation.FromJson(node)))) { }
}
