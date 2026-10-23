using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;

public class AsyncLifecycleTests
{
    [Test]
    public async Task SetupAndAsyncReadReceiveTokenAndDispose()
    {
        using var cancellation = new CancellationTokenSource();
        var reader = new Mock<DbDataReader>();
        reader.SetupSequence(x => x.ReadAsync(cancellation.Token)).ReturnsAsync(true).ReturnsAsync(false);
        reader.Setup(x => x.FieldCount).Returns(0);
        var source = new DataReaderAsArraySource(async token =>
        {
            await Task.Yield();
            Assert.That(token, Is.EqualTo(cancellation.Token));
            return ReaderLease.Own(reader.Object);
        });
        await new Pipeline(source).StartAsync(cancellation.Token);
        reader.Verify(x => x.ReadAsync(cancellation.Token), Times.Exactly(2));
        reader.Protected().Verify("Dispose", Times.Once(), new object[] { true });
    }

    [Test]
    public async Task CancellationAfterSetupDisposesOwnedReaderWithoutCompletion()
    {
        using var cancellation = new CancellationTokenSource();
        var reader = new Mock<IDataReader>();
        var source = new DataReaderAsArraySource(token =>
        {
            cancellation.Cancel();
            return ValueTask.FromResult(ReaderLease.Own(reader.Object));
        });
        var complete = 0; source.RegisterOnCompleted(() => complete++);
        try { await source.StartAsync(cancellation.Token); Assert.Fail("Expected cancellation"); }
        catch (OperationCanceledException) { }
        reader.Verify(x => x.Dispose(), Times.Once());
        Assert.That(complete, Is.Zero);
    }

    [Test]
    public async Task ConcurrentRunsAreRejectedAndRestartCanSucceed()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new DataReaderAsArraySource(async token =>
        {
            entered.TrySetResult();
            await release.Task;
            return ReaderLease.Own(new Mock<IDataReader>().Object);
        });
        var pipeline = new Pipeline(source);
        var first = pipeline.StartAsync();
        await entered.Task;
        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.StartAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => source.StartAsync());
        release.SetResult();
        await first;
        await pipeline.StartAsync();
    }

    [Test]
    public async Task SetupFailureIsObservedAndAllowsRetry()
    {
        var calls = 0;
        var source = new DataReaderAsArraySource(async token =>
        {
            await Task.Yield();
            if (++calls == 1) throw new InvalidOperationException("setup");
            return ReaderLease.Own(new Mock<IDataReader>().Object);
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => source.StartAsync());
        await source.StartAsync();
        Assert.That(calls, Is.EqualTo(2));
    }

    [Test]
    public void SynchronousEnumerableRestartsWithFreshEnumerator()
    {
        var source = new EnumerableSource<int>([1, 2]);
        Assert.That(source.GetOutputs(source.Start), Is.EqualTo(new[] { 1, 2 }));
        Assert.That(source.GetOutputs(source.Start), Is.EqualTo(new[] { 1, 2 }));
    }
}



