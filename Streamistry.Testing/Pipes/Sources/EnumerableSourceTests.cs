using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Testing.Pipes.Sources;
public class EnumerableSourceTests
{
    [Test]
    public void Read_ThreeElements_Successful()
    {
        var source = new EnumerableSource<int>([1,11,4]);
        var pipeline = new Pipeline(source);
        var sink = new MemorySink<int>(source);

        pipeline.Start();
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(4));
    }
}
