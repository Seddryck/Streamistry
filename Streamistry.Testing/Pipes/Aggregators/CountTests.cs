using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Aggregators;
public class CountTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Count<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(10);
        Assert.That(sink.State.Last(), Is.EqualTo(1));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Count<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(10);
        aggregator.Emit(15);
        aggregator.Emit(22);
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(3));
    }


    [Test]
    public void Emit_ManyElementsAsShort_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Count<int, short>(pipeline);
        var sink = new MemorySink<short>(aggregator);

        aggregator.Emit(10);
        aggregator.Emit(15);
        aggregator.Emit(22);
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo((short)3));
    }
}
