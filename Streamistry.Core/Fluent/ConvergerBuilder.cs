using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class ConvergerBuilder<TNext> : BaseRoutesBuilder
{
    protected interface IRouteBuilder<TIn, TOut> : IRouteBuilder
    {
        new IChainablePort<TOut> Build(IDualRoute dual);
    }

    public ConvergerBuilder(IBuilder<IDualRoute> upstream)
        : base(upstream)
    { }

    public ConvergerBuilder<TNext> Route(Func<IDualRoute, IChainablePort> port, ISegment segment)
    {
        Add(port, segment);
        return this;
    }

    public ConvergerBuilder<TNext> Route<TOutput>(Func<IDualRoute, IChainablePort> port, Func<BasePipeBuilder<TOutput>, BasePipeBuilder<TNext>> path)
    {
        Add(port, new Segment<TOutput, TNext>(path));
        return this;
    }

    public UnionBuilder<TNext> Union()
        => new(this);
}
