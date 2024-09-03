using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

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
        var pipeline = new Pipeline(source);
        Assert.Multiple(() =>
        {
            var results = source.GetOutputs(pipeline.Start);
            Assert.That(results, Does.Contain(1));
            Assert.That(results, Does.Contain(25));
            Assert.That(results, Does.Contain(33));
        });
    }
}
