using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Sinks;

public class MemorySinkTests
{
    [Test]
    public void Emit_DisplayOneElement_Successful()
    {
        var sink = new MemorySink<int>();
        sink.Emit(0);

        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.First(), Is.EqualTo(0));
    }

    [Test]
    public void Emit_DisplayThreeElements_Successful()
    {
        var sink = new MemorySink<string>();

        sink.Emit("Hello");
        Assert.Multiple(() =>
        {
            Assert.That(sink.State, Has.Count.EqualTo(1));
            Assert.That(sink.State.First(), Is.EqualTo("Hello"));
            Assert.That(sink.State.Last(), Is.EqualTo("Hello"));
        });

        sink.Emit("World");
        Assert.Multiple(() =>
        {
            Assert.That(sink.State, Has.Count.EqualTo(2));
            Assert.That(sink.State.First(), Is.EqualTo("Hello"));
            Assert.That(sink.State.Last(), Is.EqualTo("World"));
        });

        sink.Emit("!");
        Assert.Multiple(() =>
        {
            Assert.That(sink.State, Has.Count.EqualTo(3));
            Assert.That(sink.State.First(), Is.EqualTo("Hello"));
            Assert.That(sink.State.Last(), Is.EqualTo("!"));
        });
    }
}
