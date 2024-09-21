using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;

namespace Streamistry.Pipes.Aggregators;

public struct MinState<T>() where T : INumber<T>
{
    private bool IsEmpty { get; set; } = true;
    private T? Value { get; set; } = default;

    public MinState<T> Append(T? value)
    {
        if (value is not null)
            Value = IsEmpty ? value : T.MinNumber(Value!, value);
        IsEmpty = false;
        return this;
    }

    public readonly T? Select()
        => IsEmpty ? default : Value;
}

public class Min<TInput> : Aggregator<TInput, MinState<TInput>, TInput> where TInput : INumber<TInput>
{
    public Min(Expression<Action<Aggregator<TInput, MinState<TInput>, TInput>>>? completion = null)
        : this(completion, null)
    { }

    public Min(IChainablePipe<TInput>? upstream, Expression<Action<Aggregator<TInput, MinState<TInput>, TInput>>>? completion = null)
        : this(completion, upstream)
    { }

    protected Min(Expression<Action<Aggregator<TInput, MinState<TInput>, TInput>>>? completion = null, IChainablePipe<TInput>? upstream = null)
        : base(upstream
            , (x, y) => x.Append(y)
            , (x) => x.Select()
            , new MinState<TInput>()
            , completion)
    { }
}
