using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;

namespace Streamistry.Pipes.Aggregators;
public class Sum<TInput> : Aggregator<TInput, TInput, TInput> where TInput : INumber<TInput>
{
    public Sum(Expression<Action<Aggregator<TInput, TInput, TInput>>>? completion = null)
        : this(default, completion, null) { }

    public Sum(IChainablePipe<TInput> upstream, Expression<Action<Aggregator<TInput, TInput, TInput>>>? completion = null)
        : this(default, completion, upstream) { }

    protected Sum(TInput? seed = default, Expression<Action<Aggregator<TInput, TInput, TInput>>>? completion = null, IChainablePipe<TInput>? upstream = null)
        : base(upstream, (x, y) => x is null || y is null ? default : x += y, (x) => x, seed, completion)
    { }
}

public class Sum<TInput, TOutput> : Aggregator<TInput, TOutput, TOutput>
    where TInput : INumber<TInput>
    where TOutput : INumber<TOutput>
{
    public Sum(Expression<Action<Aggregator<TInput, TOutput, TOutput>>>? completion = null)
        : this(default, completion, null) { }

    public Sum(IChainablePipe<TInput> upstream, Expression<Action<Aggregator<TInput, TOutput, TOutput>>>? completion = null)
        : this(default, completion, upstream) { }

    protected Sum(TOutput? seed = default, Expression<Action<Aggregator<TInput, TOutput, TOutput>>>? completion = null, IChainablePipe<TInput>? upstream = null)
        : base(upstream
            , (x, y) => x is null || y is null ? default : x += TOutput.CreateChecked(y), (x) => x, seed, completion)
    { }
}
