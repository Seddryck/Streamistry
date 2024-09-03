using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry;
using Streamistry.Pipes.Sinks;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class ExceptionRouterMapperTests
{
    [Test]
    public void Emit_ValidData_MainOnly()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionMapper<int, int>(pipeline, x => 60 / x);
        Assert.Multiple(() =>
        {
            Assert.That(mapper.EmitAndGetOutput(10), Is.EqualTo(6));
            Assert.That(mapper.EmitAndGetOutput(20), Is.EqualTo(3));
            Assert.That(mapper.EmitAndGetOutput(6), Is.EqualTo(10));
        });
    }

    [Test]
    public void Emit_InvalidData_ExceptionOnly()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionMapper<int, int>(pipeline, x => 60 / x);
        Assert.Multiple(() =>
        {
            Assert.That(mapper.EmitAndAnyOutput(0), Is.False);
            Assert.That(mapper.EmitAndAnyAlternateOutput(0), Is.True);
            Assert.That(mapper.EmitAndGetAlternateOutput(0), Is.EqualTo(0));
        });
    }

    [Test]
    public void Emit_MixedData_Successful()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionMapper<int, int>(pipeline, x => 60 / x);
        Assert.Multiple(() =>
        {
            Assert.That(mapper.EmitAndGetOutput(10), Is.EqualTo(6));
            Assert.That(mapper.EmitAndGetAlternateOutput(0), Is.EqualTo(0));
            Assert.That(mapper.EmitAndGetOutput(3), Is.EqualTo(20));
        });
    }

    [Test]
    public void Emit_MixedDataWithExceptionPathAndExplicitMainPath_DontFail()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new ExceptionMapper<int, int>(pipeline, x => 60 / x);
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
