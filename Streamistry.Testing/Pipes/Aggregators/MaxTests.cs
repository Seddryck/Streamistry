﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Aggregators;
public class MaxTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Max<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(10);
        Assert.That(sink.State.Last(), Is.EqualTo(10));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var pipeline = new Pipeline<int>();
        var aggregator = new Max<int>(pipeline);
        var sink = new MemorySink<int>(aggregator);

        aggregator.Emit(15);
        aggregator.Emit(22);
        aggregator.Emit(10);
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(22));
    }
}