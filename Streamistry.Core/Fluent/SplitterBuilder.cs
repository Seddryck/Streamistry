using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class SplitterBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Func<TInput, TOutput[]>? Function { get; set; }

    public SplitterBuilder(IPipeBuilder<TInput> upstream, Func<TInput, TOutput[]>? function)
        : base(upstream)
        => (Function) = (function);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Splitter<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );
}
