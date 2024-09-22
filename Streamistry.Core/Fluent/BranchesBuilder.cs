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

public class InvalidUpstreamBranchException : InvalidOperationException
{
    public InvalidUpstreamBranchException(Type T1, Type T2)
        : base($"Input branches of the Union operators must have the same type. Following distinct types were detected '{T1.Name}' and '{T2.Name}'")
    { }
}
