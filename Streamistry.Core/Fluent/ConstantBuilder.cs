using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Mappers;

namespace Streamistry.Fluent;
public class ConstantBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    private TOutput Value { get; set; }

    public ConstantBuilder(IPipeBuilder<TInput> upstream, TOutput value)
        : base(upstream)
        => (Value) = (value);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Constant<TInput, TOutput>(
            Upstream.BuildPipeElement()
            , Value
        );
}
