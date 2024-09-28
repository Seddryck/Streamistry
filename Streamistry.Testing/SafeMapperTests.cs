using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class ExceptionRouterMapperTests
{
    [Test]
    public void Emit_ValidData_MainOnly()
    {
        var mapper = new SafeMapper<int, int>(x => 60 / x);
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
        var mapper = new SafeMapper<int, int>(x => 60 / x);
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
        var mapper = new SafeMapper<int, int>(x => 60 / x);
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
        var source = new EnumerableSource<int>([10, 0, 3]);
        var mapper = new SafeMapper<int, int>(source, x => 60 / x);
        var mainSink = new MemorySink<int>(mapper.Main);
        var exceptionSink = new MemorySink<int>(mapper.Alternate);
        source.Start();

        Assert.Multiple(() =>
        {
            Assert.That(mainSink.State.Count, Is.EqualTo(2));
            Assert.That(mainSink.State.First, Is.EqualTo(6));
            Assert.That(mainSink.State.Last, Is.EqualTo(20));
            Assert.That(exceptionSink.State.Count, Is.EqualTo(1));
            Assert.That(exceptionSink.State.First, Is.EqualTo(0));
        });
    }
}
