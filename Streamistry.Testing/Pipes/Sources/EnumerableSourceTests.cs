using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Sources;
public class EnumerableSourceTests
{
    [Test]
    public void Read_ThreeElements_Successful()
    {
        var source = new EnumerableSource<int>([1, 11, 4]);
        var pipeline = new Pipeline(source);
        Assert.That(source.GetOutputs(pipeline.Start), Is.EqualTo(new int[] { 1, 11, 4 }));
    }
}
