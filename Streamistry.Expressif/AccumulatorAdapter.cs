using global::Expressif.Accumulators;
using Streamistry.Fluent;

namespace Streamistry.Expressif;

public sealed class AccumulatorAdapter<TInput, TOutput> : SingleRouterPipe<TInput, TOutput>
{
    private readonly Func<IAccumulator> factory;
    private IAccumulator accumulator;
    public AccumulatorAdapter(IChainablePort<TInput> upstream, Func<IAccumulator> factory) : base(upstream)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        accumulator = Create();
    }
    private IAccumulator Create()
    {
        var result = factory() ?? throw new InvalidOperationException("Accumulator factory returned null.");
        result.Initialize();
        return result;
    }
    public void Reset() => accumulator = Create();
    protected override void OnRunStarting() => Reset();
    public TOutput Current => ExpressionAdapters.Adapt<TOutput>(accumulator.GetValue());
    protected override TOutput Invoke(TInput input)
    {
        accumulator.Accumulate(input);
        return Current;
    }
}

public sealed class AccumulatorBuilder<TInput, TOutput>(IPipeBuilder<TInput> upstream, Func<IAccumulator> factory)
    : PipeElementBuilder<TInput, TOutput>(upstream)
{
    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new AccumulatorAdapter<TInput, TOutput>(Upstream.BuildPipeElement(), factory);
}
