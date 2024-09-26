using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class FilterBuilder<TInput> : PipeElementBuilder<TInput, TInput>, IPipeBuilder<TInput>
{
    protected Func<TInput, bool>? Function { get; }

    public FilterBuilder(IPipeBuilder<TInput> upstream, Func<TInput, bool>? function)
        :base(upstream)
        => (Function) = (function);

    public override IChainablePort<TInput> OnBuildPipeElement()
        => new Filter<TInput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );
}

public class FilterNullBuilder<TInput> : PipeElementBuilder<TInput, object?>, IPipeBuilder<object?>
{
    public FilterNullBuilder(IPipeBuilder<TInput> upstream)
        : base(upstream)
    { }

    public override IChainablePort<object?> OnBuildPipeElement()
        => new FilterNull<TInput>(
                Upstream.BuildPipeElement()!
            );
}

public class FilterNotNullBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>, IPipeBuilder<TOutput>
    where TOutput: notnull
{
    public FilterNotNullBuilder(IPipeBuilder<TInput> upstream)
        : base(upstream)
    { }
      
    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new FilterNotNull<TInput, TOutput>(
                Upstream.BuildPipeElement()
            );
}
