using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;

namespace Streamistry.Pipes.Aggregators;

public struct AverageState<T>(T count, T total) where T : INumber<T>
{
    public AverageState<T> Append(T? value)
    {
        if (value is not null)
        {
            count += T.One;
            total += value;
        }
        return this;
    }

    public readonly T? Select()
        => count > T.Zero ? total / count : default;
}

public class Average<T> : Aggregator<T, AverageState<T>, T> where T : INumber<T>
{
    public Average(Expression<Action<Aggregator<T, AverageState<T>, T>>>? completion = null)
        : this(completion, null)
    { }

    public Average(IChainablePipe<T> upstream, Expression<Action<Aggregator<T, AverageState<T>, T>>>? completion = null)
        : this(completion, upstream)
    { }

    protected Average(Expression<Action<Aggregator<T, AverageState<T>, T>>>? completion = null, IChainablePipe<T>? upstream = null)
        : base(upstream
            , (x, y) => x.Append(y)
            , (x) => x.Select()
            , new AverageState<T>()
            , completion)
    { }
}

public class Average<T, U> : Aggregator<T, AverageState<U>, U>
        where T : INumber<T>
        where U : INumber<U>
{
    public Average(Expression<Action<Aggregator<T, AverageState<U>, U>>>? completion = null)
        : this(completion, null)
    { }

    public Average(IChainablePipe<T> upstream, Expression<Action<Aggregator<T, AverageState<U>, U>>>? completion = null)
        : this(completion, upstream)
    { }

    protected Average(Expression<Action<Aggregator<T, AverageState<U>, U>>>? completion = null, IChainablePipe<T>? upstream = null)
        : base(upstream
            , (x, y) => x.Append(y is null ? default : U.CreateChecked(y))
            , (x) => x.Select()
            , new AverageState<U>()
            , completion)
    { }
}
