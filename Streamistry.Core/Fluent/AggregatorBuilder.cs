using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Aggregators;

namespace Streamistry.Fluent;
internal class AggregatorBuilder<TInput, TAccumulate, TOutput>
{
    protected IPipeBuilder<TInput> Upstream { get; }

    public AggregatorBuilder(IPipeBuilder<TInput> upstream)
        => Upstream = upstream;

    public SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> AsMax()
        => new SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput>(Upstream, typeof(Max<>), [typeof(TInput)]);
    public SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> AsMin()
        => new SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput>(Upstream, typeof(Min<>), [typeof(TInput)]);
    public SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> AsAverage()
    => new SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput>(Upstream, typeof(Average<,>), [typeof(TInput), typeof(TOutput)]);
    public SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> AsMedian()
    => new SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput>(Upstream, typeof(Median<,>), [typeof(TInput), typeof(TOutput)]);
    public SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> AsSum()
    => new SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput>(Upstream, typeof(Sum<,>), [typeof(TInput), typeof(TOutput)]);
    public SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> AsCount()
    => new SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput>(Upstream, typeof(Count<,>), [typeof(TInput), typeof(TOutput)]);

}

internal class SpecializedAggregatorBuilder<TInput, TAccumulate, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Type Type { get; }
    protected Type[] GenericTypeParameters { get; } = [typeof(int)];
    public SpecializedAggregatorBuilder(IPipeBuilder<TInput> upstream, Type type, Type[] genericTypeParameters)
        : base(upstream)
        => (Type, GenericTypeParameters) = (type, genericTypeParameters);

    public override IChainablePort<TOutput> OnBuildPort()
    {
        var t = Type.MakeGenericType(GenericTypeParameters);
        return (IChainablePort<TOutput>)Activator.CreateInstance(t, Upstream.BuildPort(), null)!;
    }
}

internal class UniversalAggregatorBuilder<TInput, TAccumulate, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Func<TAccumulate?, TInput?, TAccumulate?>? Accumulator { get; }
    protected Func<TAccumulate?, TOutput?>? Selector { get; set; } = x => (TOutput?)Convert.ChangeType(x, typeof(TOutput));
    protected TAccumulate? Seed { get; set; } = default;

    public UniversalAggregatorBuilder(IPipeBuilder<TInput> upstream, Func<TAccumulate?, TInput?, TAccumulate?> accumulator)
        : base(upstream)
        => (Accumulator) = (accumulator);

    public UniversalAggregatorBuilder<TInput, TAccumulate, TOutput> WithSelector(Func<TAccumulate?, TOutput?>? selector)
    {
        Selector = selector;
        return this;
    }

    public UniversalAggregatorBuilder<TInput, TAccumulate, TOutput> WithSeed(TAccumulate? seed)
    {
        Seed = seed;
        return this;
    }

    public override IChainablePort<TOutput> OnBuildPort()
        => new Aggregator<TInput, TAccumulate, TOutput>(
                Upstream.BuildPort()
                , Accumulator ?? throw new InvalidOperationException()
                , Selector ?? throw new InvalidOperationException()
                , Seed
            );

}
