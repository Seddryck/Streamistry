using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public abstract class BaseRoutesBuilder : IBuilder<IChainablePort[]>
{
    protected interface IRouteBuilder
    {
        IChainablePort Build(IDualRoute dual);
    }

    protected class RouteBuilder : IRouteBuilder
    {
        protected Func<IDualRoute, IChainablePort> Port { get; }
        protected ISegment Segment { get; }

        public RouteBuilder(Func<IDualRoute, IChainablePort> port, ISegment segment)
            => (Port, Segment) = (port, segment);

        public IChainablePort Build(IDualRoute dual)
        {
            var port = Port(dual);
            var (input, output) = Segment.Craft(port.Pipe.Pipeline!);
            input.Bind(port);
            return output;
        }
    }

    protected IBuilder<IDualRoute> Upstream { get; }

    protected IChainablePort[]? Instances { get; set; }
    protected List<IRouteBuilder> RouteBuilders { get; } = [];

    public BaseRoutesBuilder(IBuilder<IDualRoute> upstream)
        => (Upstream) = (upstream);

    internal void Add(Func<IDualRoute, IChainablePort> port, ISegment segment)
        => RouteBuilders.Add(new RouteBuilder(port, segment));

    public IChainablePort[] BuildPipeElement()
        => Instances ??= OnBuildPipeElement();

    public IChainablePort[] OnBuildPipeElement()
    {
        var routes = new List<IChainablePort>();
        var upstream = Upstream.BuildPipeElement();
        foreach (var routeBuilder in RouteBuilders)
            routes.Add(routeBuilder.Build(upstream));
        return [.. routes];
    }

    public Pipeline Build()
    {
        BuildPipeElement();
        return Instances![0].Pipe.Pipeline!;
    }
}
public class RoutesBuilder : BaseRoutesBuilder
{
    public RoutesBuilder(IBuilder<IDualRoute> upstream)
        : base(upstream)
    { }

    public RoutesBuilder Route<TInput, TOutput>(Func<IDualRoute, IChainablePort> port, Segment<TInput, TOutput> segment)
    {
        Add(port, segment);
        return this;
    }

    public RoutesBuilder Route<TInput, TOutput>(Func<IDualRoute, IChainablePort> port, Func<BasePipeBuilder<TInput>, BasePipeBuilder<TOutput>> path)
    {
        Add(port, new Segment<TInput, TOutput>(path));
        return this;
    }

    public UnionBuilder<TNext> Union<TNext>()
        => new(this);
}

