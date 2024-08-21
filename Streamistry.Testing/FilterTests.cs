using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing;
public class FilterTests
{
    [Test]
    public void Emit_InlinePositive_Successful()
    {
        var pipeline = new Pipeline<int>();
        var filter = new Filter<int>(pipeline, x => x >= 0);
        var sink = new MemorySink<int>(filter);
        filter.Emit(11);

        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.First(), Is.EqualTo(11));
    }

    [Test]
    public void Emit_InlinePositive_DontEmit()
    {
        var pipeline = new Pipeline<int>();
        var filter = new Filter<int>(pipeline, x => x >= 0);
        var sink = new MemorySink<int>(filter);
        filter.Emit(-11);

        Assert.That(sink.State, Has.Count.EqualTo(0));
    }
}
