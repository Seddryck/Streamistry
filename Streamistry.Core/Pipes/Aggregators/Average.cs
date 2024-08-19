using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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

    public static readonly AverageState<T> @Default = new();
}

public class Average<T> : Aggregator<T, AverageState<T>, T> where T : INumber<T>
{
    public Average(IChainablePipe<T> upstream)
        : base(upstream
            , (x, y) => x.Append(y)
            , (x) => x.Select()
            , AverageState<T>.Default)
    { }
}

public class Average<T, U> : Aggregator<T, AverageState<U>, U>
        where T : INumber<T>
        where U : INumber<U>
{
    public Average(IChainablePipe<T> upstream)
        : base(upstream
            , (x, y) => x.Append(y is null ? default : U.CreateChecked(y))
            , (x) => x.Select()
            , AverageState<U>.Default)
    { }
}
