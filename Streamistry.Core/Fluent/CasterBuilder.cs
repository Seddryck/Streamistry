using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Mappers;

namespace Streamistry.Fluent;
public class CasterBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    public CasterBuilder(IPipeBuilder<TInput> upstream)
        : base(upstream)
    { }

    public SafeCasterBuilder<TInput, TOutput> Safe()
        => new (Upstream);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Caster<TInput, TOutput>(
                Upstream.BuildPipeElement()
            );
}

public class SafeCasterBuilder<TInput, TOutput> : CasterBuilder<TInput, TOutput>, IBuilder<IDualRoute>
{
    protected new IDualRoute? Instance { get; set; }

    public new IDualRoute BuildPipeElement()
        => Instance ??= OnBuildPipeElement() is IDualRoute dual ? dual : throw new InvalidCastException();

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new SafeCaster<TInput, TOutput>(
                Upstream.BuildPipeElement()
            );

    IDualRoute IBuilder<IDualRoute>.OnBuildPipeElement()
        => throw new InvalidOperationException();

    public SafeCasterBuilder(IPipeBuilder<TInput> upstream)
        : base(upstream)
    { }

    public RoutesBuilder Route<TPort, TNext>(Func<IDualRoute, IChainablePort> port, Segment<TPort, TNext> segment)
    {
        var routeBuilder = new RoutesBuilder(this);
        routeBuilder.Add(port, segment);
        return routeBuilder;
    }

    public RoutesBuilder Route<TPort, TNext>(Func<IDualRoute, IChainablePort> port, Func<BasePipeBuilder<TPort>, BasePipeBuilder<TNext>> path)
    {
        var routeBuilder = new RoutesBuilder(this);
        routeBuilder.Add(port, new Segment<TPort, TNext>(path));
        return routeBuilder;
    }

    public ConvergerBuilder<TNext> Converge<TNext>()
        => new(this);
}
