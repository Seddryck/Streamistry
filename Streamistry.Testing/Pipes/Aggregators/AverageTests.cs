using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Aggregators;
public class AverageTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Average<int>(pipeline);
        Assert.That(aggregator.EmitAndGetOutput(5), Is.EqualTo(5));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Average<int>(pipeline);
        Assert.That(aggregator.EmitAndAnyOutput(2), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(3), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(4), Is.True);
    }

    [Test]
    public void Emit_ManyElements_CorrectAggregation()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Average<int>(pipeline);
        aggregator.Emit(2);
        aggregator.Emit(3);
        Assert.That(aggregator.EmitAndGetOutput(4), Is.EqualTo(3));
    }

    [Test]
    public void Emit_IntTruncation_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Average<int>(pipeline);
        aggregator.Emit(2);
        Assert.That(aggregator.EmitAndGetOutput(3), Is.EqualTo(2));
    }

    [Test]
    public void Emit_IntToDecimal_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Average<int, decimal>(pipeline);
        aggregator.Emit(2);
        Assert.That(aggregator.EmitAndGetOutput(3), Is.EqualTo(2.5m));
    }
}
