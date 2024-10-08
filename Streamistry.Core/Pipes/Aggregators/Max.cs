﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;

namespace Streamistry.Pipes.Aggregators;

public struct MaxState<T>() where T : INumber<T>
{
    private bool IsEmpty { get; set; } = true;
    private T Value { get; set; } = default!;

    public MaxState<T> Append(T? value)
    {
        if (value is not null)
            Value = IsEmpty ? value : T.MaxNumber(Value!, value);
        IsEmpty = false;
        return this;
    }

    public readonly T Select()
        => IsEmpty ? default! : Value;
}

public class Max<TInput> : Aggregator<TInput, MaxState<TInput>, TInput> where TInput : INumber<TInput>
{
    public Max(Expression<Action<Aggregator<TInput, MaxState<TInput>, TInput>>>? completion = null)
        : this(completion, null)
    { }

    public Max(IChainablePipe<TInput>? upstream, Expression<Action<Aggregator<TInput, MaxState<TInput>, TInput>>>? completion = null)
        : this(completion, upstream)
    { }

    protected Max(Expression<Action<Aggregator<TInput, MaxState<TInput>, TInput>>>? completion = null, IChainablePipe<TInput>? upstream = null)
        : base(upstream
            , (x, y) => x.Append(y)
            , (x) => x.Select()
            , new MaxState<TInput>()
            , completion)
    { }
}
