using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class AggregatorTests
{
    [Test]
    public void Completion_ResetItself_AggregatorReset()
    {
        var firstSource = new EnumerableSource<int>([1, 2, 3]);
        var secondSource = new EnumerableSource<int>([-1, -2, 5]);

        var pipeline = new Pipeline(firstSource);
        var union = new Union<int>([firstSource, secondSource]);
        var aggregator = new Sum<int>(union, x => x.Reset());
        secondSource.WaitOnCompleted(aggregator);

        Assert.Multiple(() =>
        {
            var results = aggregator.GetOutputs(pipeline.Start);
            foreach (var value in (int[])[1, 3, 6, -1, -3, 2])
                Assert.That(results, Does.Contain(value));
        });
    }

    [Test]
    public void Completion_AdditionalEmit_EmitInThePipeline()
    {
        var firstSource = new EnumerableSource<int>([1, 2, 3]);
        var secondSource = new EnumerableSource<int>([-1, -2, 5]);

        var pipeline = new Pipeline(firstSource);
        var union = new Union<int>([firstSource, secondSource]);
        var aggregator = new Sum<int>(union, x => x.Emit(-x.State));
        secondSource.WaitOnCompleted(aggregator);

        Assert.Multiple(() =>
        {
            var results = aggregator.GetOutputs(pipeline.Start);
            foreach (var value in (int[])[1, 3, 6, 0, -1, -3, 2, 0])
                Assert.That(results, Does.Contain(value));
        });
    }
}
