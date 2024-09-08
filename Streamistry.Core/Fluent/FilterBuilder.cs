using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
internal class FilterBuilder<TInput> : PipeElementBuilder<TInput, TInput>, IPipeBuilder<TInput>
{
    protected Func<TInput?, bool>? Function { get; }

    public FilterBuilder(IPipeBuilder<TInput> upstream, Func<TInput?, bool>? function)
        :base(upstream)
        => (Function) = (function);

    public override IChainablePort<TInput> OnBuildPort()
        => new Filter<TInput>(
                Upstream.BuildPort()
                , Function ?? throw new InvalidOperationException()
            );
}
