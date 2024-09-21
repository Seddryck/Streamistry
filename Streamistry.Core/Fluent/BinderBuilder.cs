using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class BinderBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Segment<TInput, TOutput> Segment { get; }

    public BinderBuilder(IPipeBuilder<TInput> upstream, Segment<TInput, TOutput> segment)
        : base(upstream)
        => (Segment) = (segment);

    public override IChainablePort<TOutput> OnBuildPipeElement()
    {
        var upstream = Upstream.BuildPipeElement();
        var (input, output) = Segment.Craft(upstream.Pipe.Pipeline!);
        input.Bind(upstream);
        return output;
    }
}
