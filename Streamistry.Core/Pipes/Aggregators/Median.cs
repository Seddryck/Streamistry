using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;

namespace Streamistry.Pipes.Aggregators;

public struct MedianState<T>(int count, IEnumerable<T> list) where T : INumber<T>
{
    private bool IsOrdered { get; set; } = true;

    public MedianState<T> Append(T? value)
    {
        if (value is not null)
        {
            IsOrdered = false;
            count += 1;
            list = list?.Append(value) ?? [value];
        }
        return this;
    }

    public T? Select()
    {
        if (list is null || count == 0)
            return default;

        if (!IsOrdered)
            list = list.OrderBy(x => x);
        IsOrdered = true;

        var left = list.ElementAt((count - 1) / 2);
        if (int.IsOddInteger(count))
            return left;
        var right = list.ElementAt((count) / 2);
        if (left == right)
            return left;
        return ((left / T.CreateChecked(2)) + (right / T.CreateChecked(2)));
    }

    public static readonly MedianState<T> @Default = new();
}

public class Median<TInput> : Aggregator<TInput, MedianState<TInput>, TInput> where TInput : INumber<TInput>
{
    public Median(IChainablePipe<TInput> upstream, Expression<Action<Aggregator<TInput, MedianState<TInput>, TInput>>>? completion = null)
        : base(upstream
            , (x, y) => x.Append(y)
            , (x) => x.Select()
            , MedianState<TInput>.Default
            , completion)
    { }
}

public class Median<TInput, TOuput> : Aggregator<TInput, MedianState<TOuput>, TOuput>
        where TInput : INumber<TInput>
        where TOuput : INumber<TOuput>
{
    public Median(IChainablePipe<TInput> upstream, Expression<Action<Aggregator<TInput, MedianState<TOuput>, TOuput>>>? completion = null)
        : base(upstream
            , (x, y) => x.Append(y is null ? default : TOuput.CreateChecked(y))
            , (x) => x.Select()
            , MedianState<TOuput>.Default
            , completion)
    { }
}

