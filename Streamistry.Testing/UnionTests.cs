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
public class UnionTests
{
    [Test]
    public void Union_ManyPipes_AsSinglePipe()
    {
        
        var firstSource = new EnumerableSource<int>([1, 2, 3]);
        var secondSource = new EnumerableSource<int>([10, 20, 30]);
        var union = new Union<int>([firstSource, secondSource]);
        var sink = new MemorySink<int>(union);
        var pipeline = new Pipeline([firstSource, secondSource]);
        pipeline.Start();

        Assert.Multiple(() =>
        {
            Assert.That(sink.State, Has.Count.EqualTo(6));
            Assert.That(sink.State.First(), Is.EqualTo(1));
            Assert.That(sink.State.Last(), Is.EqualTo(30));
        });
    }
}
