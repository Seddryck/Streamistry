using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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

    public static readonly MinState<T> @Default = new();
}

public class Min<TInput> : Aggregator<TInput, MinState<TInput>, TInput> where TInput : INumber<TInput>
{
    public Min(IChainablePipe<TInput> upstream)
        : base(upstream
            , (x, y) => x.Append(y)
            , (x) => x.Select()
            , MinState<TInput>.Default)
    { }
}
