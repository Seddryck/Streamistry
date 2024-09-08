using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Observability;

namespace Streamistry.Testing.Observability;
public class ConsoleTracerTests
{
    private class ConsoleOutput : IDisposable
    {
        private readonly StringWriter stringWriter = new();
        private readonly TextWriter originalOutput = Console.Out;

        public ConsoleOutput()
            => Console.SetOut(stringWriter);

        public string GetOuput()
            => stringWriter.ToString();

        public void Dispose()
        {
            Console.SetOut(originalOutput);
            stringWriter.Dispose();
        }
    }

    [Test]
    public void Set_ConsoleTracer_Used()
    {
        var provider = new ObservabilityProvider(new ConsoleTracer());

        using var output = new ConsoleOutput();
        
        var source = new EmptySource<int>();
        var pipeline = new Pipeline(source, provider);
        var mapper = new Mapper<int, int>(source, x => ++x);
        var sink = new MemorySink<int>(mapper);
        source.Emit(0);

        Assert.That(sink.State.First(), Is.EqualTo(1)); Assert.Multiple(() =>
        {
            Assert.That(output.GetOuput(), Does.StartWith("Starting span 'Mapper "));
            Assert.That(output.GetOuput(), Does.Contain("Ending span 'Mapper "));
            Assert.That(output.GetOuput(), Does.EndWith("ticks\r\n"));
        });
    }
}
