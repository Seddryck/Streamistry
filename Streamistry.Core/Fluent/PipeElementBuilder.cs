using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;

public abstract class PipeElementBuilder<TInput, TOutput> : BasePipeBuilder<TOutput>
{
    protected IPipeBuilder<TInput> Upstream { get; }

    public PipeElementBuilder(IPipeBuilder<TInput> upstream)
        => Upstream = upstream;

    public PipeElementBuilder<TInput, TOutput> Checkpoint(out IChainablePort<TOutput> port)
    {
        port = BuildPipeElement();
        return this;
    }
}
