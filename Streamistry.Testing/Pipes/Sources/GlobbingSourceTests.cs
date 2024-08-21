using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Pipelines;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Testing.Pipes.Sources;
public class GlobbingSourceTests
{
    private const string Extension = ".glob.txt";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Tuple<string, int>[] files = [new("foo", 1), new("bar", 25), new("qwark", 33)];

        foreach (var file in files)
        {
            using var writer = new StreamWriter($"{file.Item1}{Extension}");
            writer.WriteLine(file.Item2);
            writer.Close();
        } 
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        var files = Directory.GetFiles(".", $"*{Extension}");
        foreach (var file in files)
            File.Delete(file);
    }

    [Test]
    public void Read_ThreeElements_Successful()
    {
        var source = new GlobbingSource<int>(".", $"*{Extension}");
        var pipeline = new SourcePipeline<int>(source);
        var sink = new MemorySink<int>(source);

        pipeline.Start();
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(33));
    }
}
