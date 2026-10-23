using NUnit.Framework;
using Streamistry.Pipes.Combinators;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;

public class CompletionContractTests
{
    [Test]
    public void ExhaustedInputDiscardsLargeUnmatchableTailWithoutOverflow()
    {
        var a = new EnumerableSource<int>([]);
        var b = new EnumerableSource<int>(Enumerable.Range(0, 2000));
        var zip = new Zipper<int, int, int>(a, b, (x, y) => x + y, 1);
        Assert.That(zip.GetOutputs(new Pipeline([a, b]).Start), Is.Empty);
    }

    [Test]
    public void CompletedShortInputPairsItsQueueThenDiscardsTail()
    {
        var a = new EnumerableSource<int>([10]);
        var b = new EnumerableSource<int>(Enumerable.Range(0, 2000));
        var zip = new Zipper<int, int, int>(a, b, (x, y) => x + y, 1);
        Assert.That(zip.GetOutputs(new Pipeline([a, b]).Start), Is.EqualTo(new[] { 10 }));
    }
    [Test]
    public void CapacityFlushDoesNotCompleteAndFinalFlushCompletesOnce()
    {
        var source = new EnumerableSource<int>([1, 2, 3, 4, 5]);
        var buffer = new PipeBuffer<int>(source, 2);
        var completions = 0;
        buffer.RegisterOnCompleted(() => completions++);
        Assert.That(buffer.GetOutputs(source.Start), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
        buffer.Complete();
        Assert.That(completions, Is.EqualTo(1));
    }

    [Test]
    public void UnionWaitsForEveryInputAndIgnoresRepeatedCompletion()
    {
        var a = new EmptySource<int>();
        var b = new EmptySource<int>();
        var union = new Union<int>([a, b]);
        var completed = 0;
        union.RegisterOnCompleted(() => completed++);
        a.Complete(); a.Complete();
        Assert.That(completed, Is.Zero);
        b.Complete(); b.Complete();
        Assert.That(completed, Is.EqualTo(1));
    }

    [Test]
    public void ZipPreservesNullAndDiscardsUnmatchedTail()
    {
        var a = new EmptySource<string?>();
        var b = new EmptySource<int>();
        var zip = new Zipper<string?, int, string>(a, b, (s, i) => $"{s ?? "null"}:{i}");
        a.Emit(null); a.Emit("tail"); a.Complete();
        Assert.That(zip.GetOutputs(() => { b.Emit(1); b.Complete(); }), Is.EqualTo(new[] { "null:1" }));
    }

    [TestCase(QueueOverflowPolicy.DropNewest, 1)]
    [TestCase(QueueOverflowPolicy.DropOldest, 2)]
    public void ZipSupportsLossyOverflowPolicies(QueueOverflowPolicy policy, int expected)
    {
        var a = new EmptySource<int>(); var b = new EmptySource<int>();
        var zip = new Zipper<int, int, int>(a, b, (x, y) => x, 1, policy);
        a.Emit(1); a.Emit(2);
        Assert.That(zip.GetOutputs(() => b.Emit(0)), Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void ZipFailsAtCapacityWithoutSuccessfulCompletion()
    {
        var a = new EmptySource<int>(); var b = new EmptySource<int>();
        var zip = new Zipper<int, int, int>(a, b, (x, y) => x, 1);
        var completed = 0; zip.RegisterOnCompleted(() => completed++);
        a.Emit(1);
        Assert.Throws<InvalidOperationException>(() => a.Emit(2));
        Assert.That(completed, Is.Zero);
    }

    [Test]
    public void EmptyZipCompletesAndRestartResetsAllBranchesBeforeDelivery()
    {
        var a = new EnumerableSource<int>([]); var b = new EnumerableSource<int>([1, 2]);
        var zip = new Zipper<int, int, int>(a, b, (x, y) => x + y);
        var pipeline = new Pipeline([a, b]);
        var completed = 0; zip.RegisterOnCompleted(() => completed++);
        Assert.That(zip.GetOutputs(pipeline.Start), Is.Empty);
        Assert.That(zip.GetOutputs(pipeline.Start), Is.Empty);
        Assert.That(completed, Is.EqualTo(2));
    }

    [Test]
    public void EarlyStopFlushesPartialBufferOnce()
    {
        var source = new EnumerableSource<int>([1, 2, 3]);
        var buffer = new PipeBuffer<int>(source, 10);
        source.RegisterDownstream(_ => source.Stop());
        Assert.That(buffer.GetOutputs(source.Start), Is.EqualTo(new[] { 1 }));
    }
}
