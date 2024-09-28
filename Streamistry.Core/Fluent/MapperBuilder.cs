using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class MapperBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Func<TInput, TOutput>? Function { get; set; }

    public MapperBuilder(IPipeBuilder<TInput> upstream, Func<TInput, TOutput>? function)
        : base(upstream)
        => (Function) = (function);

    public SafeMapperBuilder<TInput, TOutput> Safe()
        => new (Upstream, Function);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Mapper<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );
}

public class SafeMapperBuilder<TInput, TOutput> : MapperBuilder<TInput, TOutput>, IBuilder<IDualRoute>
{
    protected new IDualRoute? Instance { get; set; }

    public new IDualRoute BuildPipeElement()
        => Instance ??= OnBuildPipeElement();

    public new IDualRoute OnBuildPipeElement()
            => new SafeMapper<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Function ?? throw new InvalidOperationException()
            );

    public SafeMapperBuilder(IPipeBuilder<TInput> upstream, Func<TInput, TOutput>? function)
        : base(upstream, function)
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
