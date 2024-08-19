using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Streamistry.Pipes.Aggregators;
public class Sum<TInput> : Aggregator<TInput, TInput, TInput> where TInput : INumber<TInput>
{
    public Sum(IChainablePipe<TInput> upstream)
        : this(upstream, default) { }

    private Sum(IChainablePipe<TInput> upstream, TInput? seed = default)
        : base(upstream, (x, y) => x is null || y is null ? default : x += y, (x) => x, seed)
    { }
}

public class Sum<TInput, TOutput> : Aggregator<TInput, TOutput, TOutput>
    where TInput : INumber<TInput>
    where TOutput : INumber<TOutput>
{
    public Sum(IChainablePipe<TInput> upstream)
        : this(upstream, default) { }

    private Sum(IChainablePipe<TInput> upstream, TOutput? seed = default)
        : base(upstream
            , (x, y) => x is null || y is null ? default : x += TOutput.CreateChecked(y), (x) => x, seed)
    { }
}
