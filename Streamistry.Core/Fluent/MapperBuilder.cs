using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class MapperBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>, ISafeBuilder<MapperBuilder<TInput, TOutput>>
{
    protected Func<TInput, TOutput>? Function { get; set; }
    private bool IsSafe { get; set; } = false;

    public MapperBuilder(IPipeBuilder<TInput> upstream, Func<TInput, TOutput>? function)
        : base(upstream)
        => (Function) = (function);

    public MapperBuilder<TInput, TOutput> Safe()
    {
        IsSafe = true;
        return this;
    }

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => IsSafe
            ? new ExceptionMapper<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            )
            : new Mapper<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );
}
