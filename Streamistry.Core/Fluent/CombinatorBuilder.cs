using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public abstract class BaseCombinatorBuilder<TOutput> : BasePipeBuilder<TOutput>
{
    protected IBuilder<IChainablePort[]> Upstream { get; }

    public BaseCombinatorBuilder(IBuilder<IChainablePort[]> upstream)
        : base()
        => (Upstream) = (upstream);

    public BasePipeBuilder<TOutput> Checkpoint(out IChainablePort<TOutput> port)
    {
        port = BuildPipeElement();
        return this;
    }
}
