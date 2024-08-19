using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Aggregators;
public class MedianTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Median<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(5);
        Assert.That(sink.State.Last(), Is.EqualTo(5));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Median<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(1);
        aggregator.Emit(3);
        aggregator.Emit(45);
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(3));
    }

    [Test]
    public void Emit_IntTruncation_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Median<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(2);
        aggregator.Emit(3);
        Assert.That(sink.State.Last(), Is.EqualTo(2));
    }

    [Test]
    public void Emit_IntToDecimal_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Median<int, decimal>(pipeline);
        var sink = new MemorySink<decimal>(aggregator);

        aggregator.Emit(2);
        aggregator.Emit(3);
        Assert.That(sink.State.Last(), Is.EqualTo(2.5m));
    }
}
