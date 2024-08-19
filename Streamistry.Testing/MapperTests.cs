using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing;
public class MapperTests
{
    [Test]
    public void Mapper_InlineIncrement_Successful()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => ++x);
        var sink = new MemorySink<int>(mapper);
        mapper.Emit(0);

        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.First(), Is.EqualTo(1));
    }

    [Test]
    public void Mapper_InlineIncrementManyTimes_Successful()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => ++x);
        var sink = new MemorySink<int>(mapper);

        mapper.Emit(0);
        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.Last(), Is.EqualTo(1));

        mapper.Emit(50);
        Assert.That(sink.State, Has.Count.EqualTo(2));
        Assert.That(sink.State.Last(), Is.EqualTo(51));

        mapper.Emit(3);
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(4));
    }

    [Test]
    public void Mapper_InlineLengthManyTimes_Successful()
    {
        var pipeline = new Pipeline<string>();
        var mapper = new Mapper<string, int>(pipeline, x => x?.Length ?? 0);
        var sink = new MemorySink<int>(mapper);

        mapper.Emit("Hello");
        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.Last(), Is.EqualTo(5));

        mapper.Emit("World");
        Assert.That(sink.State, Has.Count.EqualTo(2));
        Assert.That(sink.State.Last(), Is.EqualTo(5));

        mapper.Emit("!");
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(1));
    }

    [Test]
    public void Mapper_InlineLengthNull_Successful()
    {
        var pipeline = new Pipeline<string>();
        var mapper = new Mapper<string, int>(pipeline, x => x?.Length ?? 0);
        var sink = new MemorySink<int>(mapper);

        mapper.Emit(null);
        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.Last(), Is.EqualTo(0));
    }

    [Test]
    public void Mapper_ChainInlineMappers_Successful()
    {
        var pipeline = new Pipeline<string>();
        var length = new Mapper<string, int>(pipeline, x => x?.Length ?? 0);
        var asterisk = new Mapper<int, string>(length, x => new string('*', x));
        var sink = new MemorySink<string>(asterisk);

        pipeline.Emit("Hello world!");
        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.Last(), Is.EqualTo("************"));
    }

    [Test]
    public void Mapper_MultipleInlineMappers_Successful()
    {
        var pipeline = new Pipeline<string>();
        var length = new Mapper<string, int>(pipeline, x => x?.Length ?? 0);
        var asterisk = new Mapper<string, string>(pipeline, x => $"***{x}***");
        var lengthSink = new MemorySink<int>(length);
        var asteriskSink = new MemorySink<string>(asterisk);

        pipeline.Emit("Hello world!");
        Assert.Multiple(() =>
        {
            Assert.That(lengthSink.State, Has.Count.EqualTo(1));
            Assert.That(lengthSink.State.Last(), Is.EqualTo(12));
            Assert.That(asteriskSink.State, Has.Count.EqualTo(1));
            Assert.That(asteriskSink.State.Last(), Is.EqualTo("***Hello world!***"));
        });
    }
}
