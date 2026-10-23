using global::Expressif;
using global::Expressif.Functions;
using global::Expressif.Predicates;
using Streamistry.Fluent;

namespace Streamistry.Expressif;

/// <summary>Adapts Expressif values to CLR ports without adding coercion rules.</summary>
public static class ExpressionAdapters
{
    public static T Adapt<T>(object? value)
    {
        if (value is T result) return result;
        if (value is null && default(T) is null) return default!;
        throw new InvalidCastException($"Expression output cannot be assigned to {typeof(T)}.");
    }

    public static Func<TInput, TOutput> Map<TInput, TOutput>(IExpression expression, EvaluationContext? context = null)
    {
        var bound = expression.WithContext(context ?? EvaluationContext.Empty);
        return value => Adapt<TOutput>(bound.Evaluate(value));
    }

    public static Func<TInput, bool> Predicate<TInput>(IFunction predicate)
        => value => Adapt<bool>(predicate.Evaluate(value));

    public static Func<TInput, bool> Predicate<TInput>(IPredicate predicate)
        => value => predicate.Evaluate(value);

    public static Mapper<TInput, TOutput> MapExpression<TInput, TOutput>(this IChainablePort<TInput> upstream, string definition, EvaluationContext? context = null)
        => upstream.MapExpression<TInput, TOutput>(Expression.Create(definition), context);

    public static Mapper<TInput, TOutput> MapExpression<TInput, TOutput>(this IChainablePort<TInput> upstream, IExpression expression, EvaluationContext? context = null)
        => new(upstream, Map<TInput, TOutput>(expression, context));

    public static Filter<TInput> FilterExpression<TInput>(this IChainablePort<TInput> upstream, string definition, EvaluationContext? context = null)
        => new(upstream, Predicate<TInput>(Expression.Create(definition).WithContext(context ?? EvaluationContext.Empty)));

    public static Filter<TInput> FilterExpression<TInput>(this IChainablePort<TInput> upstream, IPredicate predicate)
        => new(upstream, Predicate<TInput>(predicate));

    public static ExpressifBuilder<TInput> UsingExpressif<TInput>(this IPipeBuilder<TInput> upstream, EvaluationContext? context = null)
        => new(upstream, context ?? EvaluationContext.Empty);
}

/// <summary>Opt-in fluent expression/predicate adapters; definitions bind once at construction.</summary>
public sealed class ExpressifBuilder<TInput>(IPipeBuilder<TInput> upstream, EvaluationContext context)
{
    public AccumulatorBuilder<TInput, TOutput> Aggregate<TOutput>(Func<global::Expressif.Accumulators.IAccumulator> factory)
        => new(upstream, factory);
    public AccumulatorBuilder<TInput, int> Count() => Aggregate<int>(() => new global::Expressif.Accumulators.CountAccumulator());
    public AccumulatorBuilder<TInput, decimal> Sum() => Aggregate<decimal>(() => new global::Expressif.Accumulators.SumAccumulator());
    public AccumulatorBuilder<TInput, decimal?> Min() => Aggregate<decimal?>(() => new global::Expressif.Accumulators.MinAccumulator());
    public AccumulatorBuilder<TInput, decimal?> Max() => Aggregate<decimal?>(() => new global::Expressif.Accumulators.MaxAccumulator());
    public MapperBuilder<TInput, TOutput> Pluck<TOutput>(string expression)
    {
        var bound = Expression.Create(expression).WithContext(context);
        return new(upstream, value => ExpressionAdapters.Adapt<TOutput>(bound.Evaluate(StructuredValues.FromClr(value))));
    }
    public SplitterBuilder<TInput, TOutput> Split<TOutput>(string expression)
    {
        var bound = Expression.Create(expression).WithContext(context);
        return new(upstream, value => StructureAdapters.SplitResult<TOutput>(bound.Evaluate(StructuredValues.FromClr(value))));
    }
    public MapperBuilder<TInput, TOutput> Constant<TOutput>(TOutput value)
        => new(upstream, ExpressionAdapters.Map<TInput, TOutput>(Expression.Create("@constant"),
            new EvaluationContext(new Dictionary<string, object?> { ["constant"] = value })));
    public FilterBuilder<TInput> IsNull() => Filter("is-null");
    public FilterBuilder<TInput> IsNotNull() => Filter("is-not-null");
    public MapperBuilder<TInput, TOutput> Cast<TOutput>()
        => new(upstream, ScalarAdapters.Convert<TInput, TOutput>());
    public MapperBuilder<TInput, DateOnly?> Date() => Cast<DateOnly?>();
    public MapperBuilder<TInput, DateTime?> DateTime() => Cast<DateTime?>();
    public MapperBuilder<TInput, TOutput> Map<TOutput>(string definition)
        => Map<TOutput>(Expression.Create(definition));
    public MapperBuilder<TInput, TOutput> Map<TOutput>(IExpression expression)
        => new(upstream, ExpressionAdapters.Map<TInput, TOutput>(expression, context));
    public FilterBuilder<TInput> Filter(string definition)
        => new(upstream, ExpressionAdapters.Predicate<TInput>(Expression.Create(definition).WithContext(context)));
    public FilterBuilder<TInput> Filter(IPredicate predicate)
        => new(upstream, ExpressionAdapters.Predicate<TInput>(predicate));
}
