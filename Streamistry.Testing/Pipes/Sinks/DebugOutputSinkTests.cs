using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Sinks;

public class DebugOutputSinkTests
{
    [Test]
    public void Emit_DisplayOneElement_Successful()
    {
        using var output = new ConsoleOutput();

        var pipeline = new Pipeline<int>();
        var sink = new DebugOutputSink<int>(pipeline);
        sink.Emit(0);

        Assert.That(output.GetOuput(), Is.EqualTo(">>> 0\r\n"));
    }

    [Test]
    public void Emit_DisplayThreeElements_Successful()
    {
        using var output = new ConsoleOutput();

        var pipeline = new Pipeline<string>();
        var sink = new DebugOutputSink<string>(pipeline);
        sink.Emit("Hello");
        sink.Emit("World");
        sink.Emit("!");

        Assert.That(output.GetOuput(), Is.EqualTo(">>> Hello\r\n>>> World\r\n>>> !\r\n"));
    }
}
