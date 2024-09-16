using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class FilterBuilder<TInput> : PipeElementBuilder<TInput, TInput>, IPipeBuilder<TInput>
{
    protected Func<TInput?, bool>? Function { get; }

    public FilterBuilder(IPipeBuilder<TInput> upstream, Func<TInput?, bool>? function)
        :base(upstream)
        => (Function) = (function);

    public override IChainablePort<TInput> OnBuildPipeElement()
        => new Filter<TInput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );
}
