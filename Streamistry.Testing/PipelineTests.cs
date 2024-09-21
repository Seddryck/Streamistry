using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class PipelineTests
{
    [Test]
    public void Start_StraightPipeline_Successful()
    {
        var source = new EnumerableSource<int>([0, 1, 10]);
        var pipeline = new Pipeline(source);
        var incrementMap = new Mapper<int, int>(source, x => ++x);

        var outputs = incrementMap.GetOutputs(pipeline.Start);
        Assert.That(outputs, Does.Contain(1));
        Assert.That(outputs, Does.Contain(2));
        Assert.That(outputs, Does.Contain(11));
    }

    [Test]
    public void Start_BindSegment_Successful()
    {
        var source = new EnumerableSource<int>([0, 1, 10]);
        var pipeline = new Pipeline(source);
        var incrementMap = new Mapper<int, int>(source, x => ++x);

        var doubleMap = new Mapper<int, int>(x => x * 2);
        var decrementMap = new Mapper<int, int>(doubleMap, x => --x);
        doubleMap.Bind(incrementMap.Main);

        var outputs = decrementMap.GetOutputs(pipeline.Start);
        Assert.That(outputs, Does.Contain(1));
        Assert.That(outputs, Does.Contain(3));
        Assert.That(outputs, Does.Contain(21));
    }

    [Test]
    public void Start_UnbindSegment_Successful()
    {
        var source = new EnumerableSource<int>([0, 1, 10]);
        var pipeline = new Pipeline(source);
        var incrementMap = new Mapper<int, int>(source, x => ++x);
        var doubleMap = new Mapper<int, int>(incrementMap, x => x * 2);
        var decrementMap = new Mapper<int, int>(doubleMap, x => --x);
        doubleMap.Unbind(incrementMap.Main);

        var outputs = doubleMap.GetOutputs(pipeline.Start);
        Assert.That(outputs, Is.Empty);
    }

    [Test]
    public void Start_UnbindAndBindSegment_Successful()
    {
        var source = new EnumerableSource<int>([0, 1, 10]);
        var pipeline = new Pipeline(source);
        var incrementMap = new Mapper<int, int>(source, x => ++x);
        var doubleMap = new Mapper<int, int>(incrementMap, x => x * 2);
        var decrementMap = new Mapper<int, int>(doubleMap, x => --x);
        doubleMap.Unbind(incrementMap.Main);
        decrementMap.Bind(incrementMap.Main);

        var outputs = decrementMap.GetOutputs(pipeline.Start);
        Assert.That(outputs, Does.Contain(0));
        Assert.That(outputs, Does.Contain(1));
        Assert.That(outputs, Does.Contain(10));
    }
}
