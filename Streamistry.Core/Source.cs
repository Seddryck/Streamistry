using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that applies a predicate function to each element within a batch of this stream.
/// The output stream is composed of elements that satisfy the predicate; elements that do not satisfy the predicate are excluded from the downstream stream.
/// </summary>
/// <typeparam name="TOutput">The type of the elements in both the input and output streams.</typeparam>
public abstract class Source<TOutput> : ChainablePipe<TOutput>, ISource, IDisposable
{
    protected Source(ObservabilityProvider? provider)
        : base(provider)
    { }

    protected Source(Pipeline pipeline)
        : base(pipeline.GetObservabilityProvider())
    {
        Pipeline = pipeline;
    }

    private bool IsStarted { get; set; }

    public void Start()
    {
        if (IsStarted)
            return;
        IsStarted = true;
        Setup();
        try
        { Read(); }
        finally
        { Stop(); }
    }

    public void Stop()
    {
        Cleanup();
        IsStarted = false;
    }

    public virtual void Setup()
    { }

    public virtual void Cleanup()
    { }

    protected virtual void Read()
    {
        while (IsStarted && TryReadNext(out var item))
            PushDownstream(item);
        PushComplete();
    }

    protected abstract bool TryReadNext(out TOutput? item);

    public void WaitOnPrepared(IPreparablePipe pipe)
        => pipe.RegisterOnPrepared(Start);

    public void WaitOnCompleted(IChainablePipe pipe)
        => pipe.RegisterOnCompleted(Start);

    #region IDisposable
    private bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
            DisposeManagedObjects();

        DisposeUnmanagedObjects();

        disposed = true;
    }

    protected virtual void DisposeManagedObjects()
    {
        Stop();
    }

    protected virtual void DisposeUnmanagedObjects()
    { }
    #endregion
}

public class EmptySource<TOutput> : Source<TOutput>, IProcessablePipe<TOutput>
{
    public EmptySource()
        :base((ObservabilityProvider?)null)
    { }

    public void Emit(TOutput? obj)
        => PushDownstream(obj);

    protected override bool TryReadNext(out TOutput? item) => throw new NotImplementedException();
}
