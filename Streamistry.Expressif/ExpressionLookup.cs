using global::Expressif;
using global::Expressif.Values;

namespace Streamistry.Expressif;

/// <summary>Prepares a string-keyed lookup; delegates field calculation and fallback to Expressif.</summary>
public sealed class ExpressionLookup<TOutput> : Mapper<string, TOutput>, IPreparablePipe
{
    private RecordValue lookup = new();
    private bool ready;
    private Action? prepared;
    private readonly IExpression field = Expression.Create(".lookup | field(^.key)");
    private readonly IExpression fallback;

    public ExpressionLookup(IChainablePipe<string> upstream,
        IChainablePipe<KeyValuePair<string, object?>> dictionary, string fallbackExpression = "#null")
        : base(upstream, _ => default!)
    {
        fallback = Expression.Create(fallbackExpression);
        dictionary.RegisterOnStarting(_ => { lookup = new(); ready = false; });
        dictionary.RegisterDownstream(pair => lookup.Set(pair.Key, StructuredValues.FromClr(pair.Value)), () =>
        {
            ready = true;
            prepared?.Invoke();
        });
        if (upstream is Source<string> source) prepared += source.Start;
    }

    public void RegisterOnPrepared(Action downstream) => prepared += downstream;
    protected override TOutput Invoke(string value)
    {
        if (!ready) throw new InvalidOperationException("Prepare the lookup before delivering values.");
        if (value is null || !lookup.ContainsKey(value))
            return ExpressionAdapters.Adapt<TOutput>(fallback.Evaluate(value));
        var input = new RecordValue(); input.Set("lookup", lookup); input.Set("key", value);
        return ExpressionAdapters.Adapt<TOutput>(field.Evaluate(input));
    }
}
