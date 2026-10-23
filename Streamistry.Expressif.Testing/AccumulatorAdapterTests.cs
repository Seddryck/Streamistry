using Expressif.Accumulators;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Expressif.Testing;

public class AccumulatorAdapterTests
{
    [Test]
    public void IncrementalResultsResetAndInstancesAreIndependent()
    {
        var source = new EnumerableSource<object>([3, 1, 5]);
        var sum = new AccumulatorAdapter<object, decimal>(source, () => new SumAccumulator());
        var min = new AccumulatorAdapter<object, decimal?>(source, () => new MinAccumulator());
        var max = new AccumulatorAdapter<object, decimal?>(source, () => new MaxAccumulator());
        var other = new AccumulatorAdapter<object, decimal>(new EmptySource<object>(), () => new SumAccumulator());
        Assert.That(sum.GetOutputs(source.Start), Is.EqualTo(new[] { 3m, 4m, 9m }));
        Assert.That(min.Current, Is.EqualTo(1m)); Assert.That(max.Current, Is.EqualTo(5m));
        Assert.That(other.Current, Is.Zero);
        Assert.That(sum.GetOutputs(source.Start), Is.EqualTo(new[] { 3m, 4m, 9m }));
        sum.Reset(); Assert.That(sum.Current, Is.Zero);
    }

    [Test]
    public void CountIncludesNullAndEmptySourcesEmitNothing()
    {
        var source = new EnumerableSource<object?>([null, "x", null]);
        var count = new AccumulatorAdapter<object?, int>(source, () => new CountAccumulator());
        Assert.That(count.GetOutputs(source.Start), Is.EqualTo(new[] { 1, 2, 3 }));
        var empty = new EnumerableSource<object>([]);
        var min = new AccumulatorAdapter<object, decimal?>(empty, () => new MinAccumulator());
        Assert.That(min.GetOutputs(empty.Start), Is.Empty);
        Assert.That(min.Current, Is.Null);
    }

    [Test]
    public void NumericNullAndOverflowFollowExpressif()
    {
        var source = new EmptySource<object?>();
        var sum = new AccumulatorAdapter<object?, decimal>(source, () => new SumAccumulator());
        Assert.Throws<InvalidCastException>(() => source.Emit(null));
        source.Emit(decimal.MaxValue);
        Assert.Throws<OverflowException>(() => source.Emit(1m));
    }

    [Test]
    public void MinAndMaxEmitEveryRunningResult()
    {
        var source = new EnumerableSource<object>([3, 1, 5]);
        var min = new AccumulatorAdapter<object, decimal?>(source, () => new MinAccumulator());
        var max = new AccumulatorAdapter<object, decimal?>(source, () => new MaxAccumulator());
        Assert.That(min.GetOutputs(source.Start), Is.EqualTo(new[] { 3m, 1m, 1m }));
        Assert.That(max.GetOutputs(source.Start), Is.EqualTo(new[] { 3m, 3m, 5m }));
    }
}
