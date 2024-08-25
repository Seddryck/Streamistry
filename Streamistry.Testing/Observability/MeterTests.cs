using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Observability;
using Streamistry.Observability.Meters;
using Streamistry.Observability.Thresholds;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Testing.Observability;
public class MeterTests
{
    private class ConsoleOutput : IDisposable
    {
        private readonly StringWriter stringWriter = new();
        private readonly TextWriter originalOutput = Console.Out;

        public ConsoleOutput()
            => Console.SetOut(stringWriter);

        public string GetOuput()
            => stringWriter.ToString();

        public int CountSubstring(string value)
        {
            var text = GetOuput();
            int count = 0, minIndex = text.IndexOf(value, 0);
            while (minIndex != -1)
            {
                minIndex = text.IndexOf(value, minIndex + value.Length);
                count++;
            }
            return count;
        }

        public void Dispose()
        {
            Console.SetOut(originalOutput);
            stringWriter.Dispose();
        }
    }
    
    [Test]
    public void Counter_Mapper_ReturnCount()
    {
        var observability = new ObservabilityProvider(new NullTracer());
        var publisher = new ConsolePublisher().Publish;

        var source = new EnumerableSource<int>(Enumerable.Range(1, 30), observability);
        var mapper = new Mapper<int, int>(source, x => ++x);
        var sink = new MemorySink<int>(mapper);

        var counter = new Counter(new MaxCardinality(10), publisher);
        observability.AttachMeters(mapper, [counter]);

        using var output = new ConsoleOutput();
        source.Start();

        Assert.That(output.CountSubstring("10 batches"), Is.EqualTo(3));
    }

    [Test]
    public void Rate_Mapper_ReturnRate()
    {
        var observability = new ObservabilityProvider(new NullTracer());
        var publisher = new ConsolePublisher().Publish;

        var source = new EnumerableSource<int>(Enumerable.Range(1, 30), observability);
        var mapper = new Mapper<int, int>(source, x => ++x);
        var sink = new MemorySink<int>(mapper);

        var rateMeter = new RateMeter(new MaxCardinality(10), publisher);
        observability.AttachMeters(mapper, [rateMeter]);

        using var output = new ConsoleOutput();
        source.Start();

        Assert.That(output.CountSubstring("batch/sec"), Is.EqualTo(3));
    }

    [Test]
    public void Histogram_Mapper_ReturnRate()
    {
        var observability = new ObservabilityProvider(new NullTracer());
        var publisher = new ConsolePublisher().Publish;

        var source = new EnumerableSource<int>(Enumerable.Range(1, 30), observability);
        var mapper = new Mapper<int, int>(source, x => ++x);
        var sink = new MemorySink<int>(mapper);

        static string bucketizer(int x) => x % 2 == 0 ? "even" : "odd";
        var histogram = new Histogram<int>(bucketizer, new MaxCardinality(10), publisher);
        observability.AttachMeters(mapper, [histogram]);

        using var output = new ConsoleOutput();
        source.Start();

        Assert.That(output.CountSubstring("[{odd => 5}, {even => 5}]"), Is.EqualTo(3));
    }
}
