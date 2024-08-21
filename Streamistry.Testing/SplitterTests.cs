using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing;
public class SplitterTests
{
    [Test]
    public void Mapper_InlineIncrement_Successful()
    {
        var pipeline = new Pipeline<string>();
        var mapper = new Splitter<string, string>(pipeline, x => x?.Split(';') ?? null);
        var sink = new MemorySink<string>(mapper);
        mapper.Emit("foo;bar;quark");

        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(sink.State.First(), Is.EqualTo("foo"));
            Assert.That(sink.State.Last(), Is.EqualTo("quark"));
        });
    }
}
