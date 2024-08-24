using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Streamistry.Observability;
using static System.Reflection.Metadata.BlobBuilder;

namespace Streamistry.Pipes.Sources;
public class GlobbingSource<TOutput> : Source<TOutput>
{
    private DirectoryInfoBase Directory { get; }
    private Matcher Matcher { get; } = new();
    private Queue<string>? Files { get; set; }

    public GlobbingSource(string directory, string glob)
        : this(directory, [glob]) { }

    public GlobbingSource(string directory, string[] globs)
        : this(directory, globs, []) { }

    public GlobbingSource(string directory, string[] includeGlobs, string[] excludeGlobs, ObservabilityProvider? provider = null)
        : base(provider)
    {
        Directory = new DirectoryInfoWrapper(new DirectoryInfo(directory));
        Matcher.AddIncludePatterns(includeGlobs);
        Matcher.AddExcludePatterns(excludeGlobs);
    }

    protected override bool TryReadNext(out TOutput? item)
    {
        Files ??= QueueFiles(Matcher);

        if (Files.TryDequeue(out var file))
        {
            var fullPath = Path.Combine([Directory.FullName, file]);
            using var reader = new StreamReader(fullPath);
            item = (TOutput?)Convert.ChangeType(reader.ReadToEnd(), typeof(TOutput));
            return true;
        }
        item = default;
        return false;
    }

    private Queue<string> QueueFiles(Matcher matcher)
    {
        var files = new Queue<string>();
        var results = matcher.Execute(Directory);
        foreach (var result in results.Files)
            files.Enqueue(result.Path);
        return files;
    }
}
