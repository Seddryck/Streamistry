using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class UnionTests
{
    [Test]
    public void Union_ManyPipes_AsSinglePipe()
    {
        
        var firstSource = new EnumerableSource<int>([1, 2, 3]);
        var secondSource = new EnumerableSource<int>([10, 20, 30]);
        var pipeline = new Pipeline([firstSource, secondSource]);
        var union = new Union<int>([firstSource, secondSource]);

        Assert.That(union.GetOutputs(pipeline.Start), Is.EqualTo(new int[] { 1, 2, 3, 10, 20, 30 }));
    }
}
