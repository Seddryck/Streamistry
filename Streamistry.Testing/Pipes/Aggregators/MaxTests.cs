using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Aggregators;
public class MaxTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var aggregator = new Max<int>();
        Assert.That(aggregator.EmitAndGetOutput(10), Is.EqualTo(10));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var aggregator = new Max<int>();
        Assert.That(aggregator.EmitAndAnyOutput(15), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(22), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(10), Is.True);
    }

    [Test]
    public void Emit_ManyElements_CorrectResults()
    {
        var aggregator = new Max<int>();
        Assert.That(aggregator.EmitAndGetOutput(15), Is.EqualTo(15));
        Assert.That(aggregator.EmitAndGetOutput(22), Is.EqualTo(22));
        Assert.That(aggregator.EmitAndGetOutput(10), Is.EqualTo(22));
    }

    [Test]
    public void Emit_TwoAggregatorsInParallel_CorrectResults()
    {
        var source = new EnumerableSource<int>(new[] { 15, 22, 10 });
        var pipeline = new Pipeline(source);
        var aggregator1 = new Max<int>(source);
        var filter = new Filter<int>(source, x => x < 20);
        var aggregator2 = new Max<int>(filter);

        var data = aggregator2.GetOutputs(pipeline.Start);
        Assert.That(data, Does.Not.Contain(22));
        Assert.That(data, Does.Contain(15));
    }
}
