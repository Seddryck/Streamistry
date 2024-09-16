using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class MapperBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Func<TInput?, TOutput?>? Function { get; set; }

    public MapperBuilder(IPipeBuilder<TInput> upstream, Func<TInput?, TOutput?>? function)
        : base(upstream)
        => (Function) = (function);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Mapper<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );
}
