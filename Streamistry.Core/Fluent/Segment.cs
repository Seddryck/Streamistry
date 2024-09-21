using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry.Fluent;

public class Segment<TInput, TOutput> : IPipeBuilder<TInput>
{
    private VirtualInput<TInput> Input { get; set; } = new();
    internal Func<BasePipeBuilder<TInput>, BasePipeBuilder<TOutput>> Builder { get; }

    public Segment(Func<BasePipeBuilder<TInput>, BasePipeBuilder<TOutput>> builder)
        => Builder = builder;

    public IChainablePort<TInput> BuildPipeElement()
        => Input ??= (VirtualInput<TInput>)OnBuildPipeElement();

    public IChainablePort<TInput> OnBuildPipeElement()
        => new VirtualInput<TInput>();

    public (IBindablePipe<TInput> input, IChainablePort<TOutput>) Craft(Pipeline pipeline)
    {
        Input.Pipeline = pipeline;
        var builder = Builder.Invoke(Input);
        builder.Build();
        return (Input.GetTarget(), builder.BuildPipeElement());
    }

    private class VirtualInput<T> : BasePipeBuilder<T>, IChainablePort<T>, IChainablePipe
    {
        private IBindablePipe<T>? Target { get; set; }

        public IChainablePipe Pipe
            => this;

        public Pipeline? Pipeline { get; set; }

        public IBindablePipe<T> GetTarget()
            => Target ?? throw new InvalidOperationException();

        public void RegisterDownstream(Action<T?> downstream)
            => Target ??= (IBindablePipe<T>)downstream.Target!;

        public void UnregisterDownstream(Action<T?> downstream)
            => Target ??= (IBindablePipe<T>)downstream.Target!;


        public void RegisterOnCompleted(Action? complete)
        {
            if (complete is not null)
                Target ??= (IBindablePipe<T>)complete.Target!;
        }

        public override IChainablePort<T> OnBuildPipeElement()
            => this;

        public void RegisterObservability(ObservabilityProvider? provider) => throw new NotImplementedException();
        public ObservabilityProvider? GetObservabilityProvider() => null;
    }
}


