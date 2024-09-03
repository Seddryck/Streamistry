using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class PipeBufferTests
{
    [Test]
    public void WithoutBuffer_TwoSinks_Overlaps()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => x + 1);
        var firstSink = new DebugOutputSink<int>(mapper);
        var secondSink = new DebugOutputSink<int>(mapper);

        using var output = new ConsoleOutput();

        pipeline.Emit(0);
        pipeline.Emit(1);
        pipeline.Emit(2);
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 1\r\n>>> 2\r\n>>> 2\r\n>>> 3\r\n>>> 3\r\n"));
    }

    [Test]
    public void WithBuffer_TwoSinks_NoOverlap()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => x + 1);
        var firstSink = new DebugOutputSink<int>(mapper);
        var buffer = new PipeBuffer<int>(mapper);
        var secondSink = new DebugOutputSink<int>(buffer);

        using var output = new ConsoleOutput();

        pipeline.Emit(0);
        pipeline.Emit(1);
        pipeline.Emit(2);
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n>>> 3\r\n"));

        pipeline.Complete();
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 1\r\n>>> 2\r\n>>> 3\r\n"));
    }

    [Test]
    public void WithBuffer_TwoSinks2_NoOverlap()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => x + 1);
        var buffer = new PipeBuffer<int>(mapper);

        bool[] expected = [true, false];
        Assert.Multiple(() =>
        {
            Assert.That(mapper.EmitAndAnyOutputs(0, [mapper, buffer]), Is.EqualTo(expected));
            Assert.That(mapper.EmitAndAnyOutputs(1, [mapper, buffer]), Is.EqualTo(expected));
            Assert.That(mapper.EmitAndAnyOutputs(2, [mapper, buffer]), Is.EqualTo(expected));

            Assert.That(buffer.GetOutputs(pipeline.Complete), Is.EqualTo(new int[] { 1, 2, 3 }));
        });
    }

    [Test]
    public void WithBufferMaxCapacity_TwoSinks_NoOverlap()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => x + 1);
        var buffer = new PipeBuffer<int>(mapper, 3);

        bool[] blocked = [true, false];
        bool[] released = [true, true];
        Assert.Multiple(() =>
        {
            Assert.That(mapper.EmitAndAnyOutputs(0, [mapper, buffer]), Is.EqualTo(blocked));
            Assert.That(mapper.EmitAndAnyOutputs(1, [mapper, buffer]), Is.EqualTo(blocked));
            Assert.That(mapper.EmitAndAnyOutputs(2, [mapper, buffer]), Is.EqualTo(released));
            Assert.That(mapper.EmitAndAnyOutputs(3, [mapper, buffer]), Is.EqualTo(blocked));
            Assert.That(buffer.GetOutputs(pipeline.Complete), Is.EqualTo(new int[] { 4 }));
        });
    }

    [Test]
    public void WithBufferMaxCapacity_TwoSources_NoOverlapSourcePushCompletion()
    {
        var source = new EnumerableSource<int>(Enumerable.Range(0, 4));
        var mapper = new Mapper<int, int>(source, x => x + 1);
        var buffer = new PipeBuffer<int>(mapper, 3);
        Assert.That(buffer.GetOutputs(source.Start), Is.EqualTo(new int[] { 1, 2, 3, 4 }));
    }
}
