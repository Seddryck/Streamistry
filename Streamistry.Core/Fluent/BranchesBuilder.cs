using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public abstract class BranchesBuilder<TInput> : IBuilder<IChainablePort[]>
{
    protected BasePipeBuilder<TInput> Upstream { get; }
    protected IChainablePort[]? Instances { get; set; }

    public BranchesBuilder(BasePipeBuilder<TInput> upstream)
        => (Upstream) = (upstream);

    public IChainablePort[] BuildPipeElement()
        => Instances ??= OnBuildPipeElement();

    public abstract IChainablePort[] OnBuildPipeElement();

    public Pipeline Build()
    {
        BuildPipeElement();
        return Instances![0].Pipe.Pipeline!;
    }

}

public partial class BranchesBuilder<TInput, T1, T2> : BranchesBuilder<TInput>
{
    
}
