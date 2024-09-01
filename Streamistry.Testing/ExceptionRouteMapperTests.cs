using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing;
public class ExceptionRouterMapperTests
{

    [Test]
    public void Emit_ValidData_MainOnly()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionRouterMapper<int, int>(pipeline, x => 60 / x);
        var mainSink = new MemorySink<int>(mapper);
        var exceptionSink = new MemorySink<int>(mapper.Alternate);
        pipeline.Emit(10);
        pipeline.Emit(20);
        pipeline.Emit(6);

        Assert.That(mainSink.State.Count, Is.EqualTo(3));
        Assert.That(mainSink.State.First, Is.EqualTo(6));
        Assert.That(mainSink.State.Last, Is.EqualTo(10));
        Assert.That(exceptionSink.State.Count, Is.EqualTo(0));
    }

    [Test]
    public void Emit_InvalidData_ExceptionOnly()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionRouterMapper<int, int>(pipeline, x => 60 / x);
        var mainSink = new MemorySink<int>(mapper);
        var exceptionSink = new MemorySink<int>(mapper.Alternate);
        pipeline.Emit(0);

        Assert.That(mainSink.State.Count, Is.EqualTo(0));
        Assert.That(exceptionSink.State.Count, Is.EqualTo(1));
        Assert.That(exceptionSink.State.First, Is.EqualTo(0));
    }

    [Test]
    public void Emit_MixedDataNoExceptionPath_DontFail()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionRouterMapper<int, int>(pipeline, x => 60 / x);
        var mainSink = new MemorySink<int>(mapper);
        pipeline.Emit(10);
        pipeline.Emit(0);
        pipeline.Emit(3);

        Assert.That(mainSink.State.Count, Is.EqualTo(2));
        Assert.That(mainSink.State.First, Is.EqualTo(6));
        Assert.That(mainSink.State.Last, Is.EqualTo(20));
    }

    [Test]
    public void Emit_MixedDataWithExceptionPath_DontFail()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionRouterMapper<int, int>(pipeline, x => 60 / x);
        var mainSink = new MemorySink<int>(mapper);
        var exceptionSink = new MemorySink<int>(mapper.Alternate);
        pipeline.Emit(10);
        pipeline.Emit(0);
        pipeline.Emit(3);

        Assert.That(mainSink.State.Count, Is.EqualTo(2));
        Assert.That(mainSink.State.First, Is.EqualTo(6));
        Assert.That(mainSink.State.Last, Is.EqualTo(20));
        Assert.That(exceptionSink.State.Count, Is.EqualTo(1));
        Assert.That(exceptionSink.State.First, Is.EqualTo(0));
    }

    [Test]
    public void Emit_MixedDataWithExceptionPathAndExplicitMainPath_DontFail()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionRouterMapper<int, int>(pipeline, x => 60 / x);
        var mainSink = new MemorySink<int>(mapper.Main);
        var exceptionSink = new MemorySink<int>(mapper.Alternate);
        pipeline.Emit(10);
        pipeline.Emit(0);
        pipeline.Emit(3);

        Assert.That(mainSink.State.Count, Is.EqualTo(2));
        Assert.That(mainSink.State.First, Is.EqualTo(6));
        Assert.That(mainSink.State.Last, Is.EqualTo(20));
        Assert.That(exceptionSink.State.Count, Is.EqualTo(1));
        Assert.That(exceptionSink.State.First, Is.EqualTo(0));
    }
}
