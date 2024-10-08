﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;

namespace Streamistry.Pipes.Aggregators;
public class Count<T> : Aggregator<T, int, int>
{
    public Count(Expression<Action<Aggregator<T, int, int>>>? completion = null)
        : this(completion, null)
    { }

    public Count(IChainablePipe<T> upstream, Expression<Action<Aggregator<T, int, int>>>? completion = null)
        : this(completion, upstream)
    { }

    protected Count(Expression<Action<Aggregator<T, int, int>>>? completion = null, IChainablePipe<T>? upstream = null)
        : base(upstream
            , (x, y) => y is null ? x : ++x, (x) => x, 0, completion)
    { }
}

public class Count<T, U> : Aggregator<T, U, U>
    where U : INumber<U>
{
    public Count(Expression<Action<Aggregator<T, U, U>>>? completion = null)
        : this(completion, null)
    { }

    public Count(IChainablePipe<T> upstream, Expression<Action<Aggregator<T, U, U>>>? completion = null)
        : this(completion, upstream)
    { }

    protected Count(Expression<Action<Aggregator<T, U, U>>>? completion = null, IChainablePipe<T>? upstream = null)
        : base(upstream
            , (x, y) => y is null ? x : (x is null ? U.One : ++x), (x) => x, U.Zero, completion)
    { }
}
