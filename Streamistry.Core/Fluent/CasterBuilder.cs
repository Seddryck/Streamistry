using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Mappers;

namespace Streamistry.Fluent;
public class CasterBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>, ISafeBuilder<CasterBuilder<TInput, TOutput>>
{
    private bool IsSafe { get; set; } = false;

    public CasterBuilder(IPipeBuilder<TInput> upstream)
        : base(upstream)
    { }

    public CasterBuilder<TInput, TOutput> Safe()
    {
        IsSafe = true;
        return this;
    }

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => IsSafe
            ? new SafeCaster<TInput, TOutput>(
                Upstream.BuildPipeElement()
            )
            : new Caster<TInput, TOutput>(
                Upstream.BuildPipeElement()
            );
}


