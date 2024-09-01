using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Parsers;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing;
public class ParserTests
{
    [Test]
    public void DateParserEmit_ValidData_MainRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateParser(pipeline);
        var mainSink = new MemorySink<DateOnly>(parser);
        pipeline.Emit("2024-08-30");
        pipeline.Emit("2024-08-31");
        pipeline.Emit("2024-09-01");

        Assert.That(mainSink.State, Has.Count.EqualTo(3));
        Assert.That(mainSink.State.First(), Is.EqualTo(new DateOnly(2024, 8, 30)));
        Assert.That(mainSink.State.Last(), Is.EqualTo(new DateOnly(2024, 9, 1)));
    }

    [Test]
    public void DateParserEmit_MixedData_MainRouteAndExceptionRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateParser(pipeline);
        var mainSink = new MemorySink<DateOnly>(parser);
        var exceptionSink = new MemorySink<string>(parser.Alternate);
        pipeline.Emit("2024-08-30");
        pipeline.Emit("2024-08-31");
        pipeline.Emit("2024-08-32");

        Assert.That(mainSink.State, Has.Count.EqualTo(2));
        Assert.That(mainSink.State.First(), Is.EqualTo(new DateOnly(2024, 8, 30)));
        Assert.That(mainSink.State.Last(), Is.EqualTo(new DateOnly(2024, 8, 31)));
        Assert.That(exceptionSink.State, Has.Count.EqualTo(1));
        Assert.That(exceptionSink.State.First(), Is.EqualTo("2024-08-32"));
    }

    [Test]
    public void DateTimeParserEmit_ValidData_MainRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateTimeParser(pipeline);
        var mainSink = new MemorySink<DateTime>(parser);
        pipeline.Emit("2024-08-30 17:12:16");
        pipeline.Emit("2024-08-31T17:12:16");
        pipeline.Emit("2024-09-01 05:00AM");

        Assert.That(mainSink.State, Has.Count.EqualTo(3));
        Assert.That(mainSink.State.First(), Is.EqualTo(new DateTime(2024, 8, 30, 17, 12, 16)));
        Assert.That(mainSink.State.Last(), Is.EqualTo(new DateTime(2024, 9, 1, 5, 0, 0)));
    }

    [Test]
    public void DateTimeParserEmit_MixedData_MainRouteAndExceptionRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateTimeParser(pipeline);
        var mainSink = new MemorySink<DateTime>(parser);
        var exceptionSink = new MemorySink<string>(parser.Alternate);
        pipeline.Emit("2024-08-30 17:12:16");
        pipeline.Emit("2024-08-31 25:62:41");
        pipeline.Emit("2024-09-01 05:00AM");

        Assert.That(mainSink.State, Has.Count.EqualTo(2));
        Assert.That(mainSink.State.First(), Is.EqualTo(new DateTime(2024, 8, 30, 17, 12, 16)));
        Assert.That(mainSink.State.Last(), Is.EqualTo(new DateTime(2024, 9, 1, 5, 0, 0)));
        Assert.That(exceptionSink.State, Has.Count.EqualTo(1));
        Assert.That(exceptionSink.State.First(), Is.EqualTo("2024-08-31 25:62:41"));
    }
}
