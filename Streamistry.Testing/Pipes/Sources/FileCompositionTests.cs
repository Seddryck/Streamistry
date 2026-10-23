using NUnit.Framework;
using Streamistry.Pipes.Mappers;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Sources;

public class FileCompositionTests
{
    [Test]
    public void DiscoveryReadingAndValueConversionAreIndependentAndRestartable()
    {
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"files-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "value.txt");
        try
        {
            File.WriteAllText(path, "12");
            var source = new FilePathSource(directory, ["*.txt"]);
            Assert.That(source.GetOutputs(source.Start), Is.EqualTo(new[] { path }));
            var text = new FileTextReader(source);
            var number = new Mapper<string, int>(text, int.Parse);
            Assert.That(number.GetOutputs(source.Start), Is.EqualTo(new[] { 12 }));
            // An exclusive handle proves text reading released the file before returning.
            using var exclusive = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); Directory.Delete(directory); }
    }
}
