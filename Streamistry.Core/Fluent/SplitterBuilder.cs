using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
internal class SplitterBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Func<TInput?, TOutput[]?>? Function { get; set; }

    public SplitterBuilder(IPipeBuilder<TInput> upstream, Func<TInput?, TOutput[]?>? function)
        : base(upstream)
        => (Function) = (function);

    public override IChainablePort<TOutput> OnBuildPort()
        => new Splitter<TInput, TOutput>(
                Upstream.BuildPort()
                , Function ?? throw new InvalidOperationException()
            );
}
