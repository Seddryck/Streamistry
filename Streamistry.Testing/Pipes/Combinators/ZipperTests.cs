using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Combinators;
public class ZipperTests
{
    [Test]
    public void Emit_ZipWhenBothEmitted_Successful()
    {
        var first = new Pipeline<char>();
        var second = new Pipeline<int>();
        var combinator = new Zipper<char, int, string>(first, second, (x, y) => new string(x, y));
        var sink = new MemorySink<string>(combinator);

        first.Emit('*');
        Assert.That(sink.State, Has.Count.EqualTo(0));

        second.Emit(5);
        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.Last(), Is.EqualTo("*****"));
    }

    [Test]
    public void Emit_ZipWhenBothEmittedNotInSync_Successful()
    {
        var first = new Pipeline<char>();
        var second = new Pipeline<int>();
        var combinator = new Zipper<char, int, string>(first, second, (x, y) => new string(x, y));
        var sink = new MemorySink<string>(combinator);

        first.Emit('*');
        first.Emit('!');
        Assert.That(sink.State, Has.Count.EqualTo(0));

        second.Emit(5);
        second.Emit(3);
        Assert.That(sink.State, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(sink.State.First(), Is.EqualTo("*****"));
            Assert.That(sink.State.Last(), Is.EqualTo("!!!"));
        });
    }
}
