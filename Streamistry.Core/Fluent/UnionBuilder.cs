using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class UnionBuilder<TInput> : BasePipeBuilder<TInput>
{
    protected IBuilder<IChainablePort[]> Upstream { get; }

    public UnionBuilder(IBuilder<IChainablePort[]> upstream)
        : base()
        => Upstream = upstream;

    public override IChainablePort<TInput> OnBuildPipeElement()
    {
        var upstreams = Upstream.BuildPipeElement().Select(x => x.Pipe);
        if (upstreams.Any(x => x is not IChainablePipe<TInput>))
            throw new InvalidOperationException();

        var pipes = upstreams.Cast<IChainablePipe<TInput>>().ToArray();

        return new Union<TInput>(pipes);
    }
}
