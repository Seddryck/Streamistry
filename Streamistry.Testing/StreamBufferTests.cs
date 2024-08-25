using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Testing;
public class StreamBufferTests
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
        var buffer = new StreamBuffer<int>(mapper);
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
    public void WithBufferMaxCapacity_TwoSinks_NoOverlap()
    {
        var pipeline = new Pipeline<int>();
        var mapper = new Mapper<int, int>(pipeline, x => x + 1);
        var firstSink = new DebugOutputSink<int>(mapper);
        var buffer = new StreamBuffer<int>(mapper, 3);
        var secondSink = new DebugOutputSink<int>(buffer);

        using var output = new ConsoleOutput();

        pipeline.Emit(0);
        pipeline.Emit(1);
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n"));
        pipeline.Emit(2);
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 1\r\n>>> 2\r\n>>> 3\r\n"));
        pipeline.Emit(3);
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 4\r\n"));
        pipeline.Complete();
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 4\r\n>>> 4\r\n"));
    }

    [Test]
    public void WithBufferMaxCapacity_TwoSources_NoOverlapSourcePushCompletion()
    {
        var source = new EnumerableSource<int>(Enumerable.Range(0, 4));
        var mapper = new Mapper<int, int>(source, x => x + 1);
        var firstSink = new DebugOutputSink<int>(mapper);
        var buffer = new StreamBuffer<int>(mapper, 3);
        var secondSink = new DebugOutputSink<int>(buffer);

        using var output = new ConsoleOutput();
        source.Start();
        Assert.That(output.GetOuput(), Is.EqualTo(">>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 1\r\n>>> 2\r\n>>> 3\r\n>>> 4\r\n>>> 4\r\n"));
    }
}
