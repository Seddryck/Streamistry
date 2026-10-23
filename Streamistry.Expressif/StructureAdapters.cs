using System.Collections;
using global::Expressif;
using global::Expressif.Values;

namespace Streamistry.Expressif;

public static partial class StructureAdapters
{
    public static TOutput[] SplitResult<TOutput>(object? result)
    {
        if (result is IEnumerable collection && result is not string && result is not RecordValue)
            return collection.Cast<object?>().Select(ExpressionAdapters.Adapt<TOutput>).ToArray();
        return [ExpressionAdapters.Adapt<TOutput>(result)];
    }

    private static Func<object?[], TOutput> Combination<TOutput>(string definition)
    {
        var expression = Expression.Create(definition);
        return values =>
        {
            var record = new RecordValue();
            for (var i = 0; i < values.Length; i++) record.Set($"item{i + 1}", StructuredValues.FromClr(values[i]));
            return ExpressionAdapters.Adapt<TOutput>(expression.Evaluate(record));
        };
    }
}
