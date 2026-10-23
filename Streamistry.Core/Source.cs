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

    private int active;
    private volatile bool stopRequested;
    private bool IsStarted => Volatile.Read(ref active) == 1 && !stopRequested;
    private Guid? preparedRun;
    public void PrepareRun(Guid runId)
    {
        if (Volatile.Read(ref active) != 0) throw new InvalidOperationException("The source is already running.");
        preparedRun = runId;
        BeginRun(runId);
    }

    private void EnterRun()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("The source is already running.");
        stopRequested = false;
    }
    private void StartRun()
    {
        BeginRun(preparedRun ?? Pipeline.CurrentRun.Value ?? Guid.NewGuid());
        preparedRun = null;
    }

    public void Start()
    {
        EnterRun();
        try
        {
            StartRun();
            Setup();
            Read();
        }
        finally
        {
            try { Cleanup(); }
            finally { Volatile.Write(ref active, 0); }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        EnterRun();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartRun();
            await SetupAsync(cancellationToken).ConfigureAwait(false);
            while (IsStarted)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var next = await ReadNextAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (!next.HasValue) break;
                if (IsStarted) PushDownstream(next.Item);
            }
            cancellationToken.ThrowIfCancellationRequested();
            PushComplete();
        }
        finally
        {
            try { await CleanupAsync().ConfigureAwait(false); }
            finally { Volatile.Write(ref active, 0); }
        }
    }

    public void Stop() => stopRequested = true;
    public virtual ValueTask SetupAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Setup();
        return ValueTask.CompletedTask;
    }
    public virtual ValueTask CleanupAsync()
    {
        Cleanup();
        return ValueTask.CompletedTask;
    }
    protected virtual ValueTask<(bool HasValue, TOutput? Item)> ReadNextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasValue = TryReadNext(out var item);
        return ValueTask.FromResult((hasValue, item));
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
