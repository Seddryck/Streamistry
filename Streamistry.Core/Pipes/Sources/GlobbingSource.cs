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

    public GlobbingSource(Pipeline upstream, string directory, string[] includeGlobs, string[] excludeGlobs)
       : base(upstream)
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
            item = ReadFile(fullPath);
            return true;
        }
        item = default;
        return false;
    }

    public override void Cleanup() => Files = null;

    protected virtual TOutput? ReadFile(string fullPath)
        => (TOutput?)Convert.ChangeType(File.ReadAllText(fullPath), typeof(TOutput));

    private Queue<string> QueueFiles(Matcher matcher)
    {
        var files = new Queue<string>();
        var results = matcher.Execute(Directory);
        foreach (var result in results.Files)
            files.Enqueue(result.Path);
        return files;
    }
}

/// <summary>Emits file paths without opening files or calculating output values.</summary>
public sealed class FilePathSource : GlobbingSource<string>
{
    public FilePathSource(string directory, string[] includes, string[]? excludes = null)
        : base(directory, includes, excludes ?? []) { }
    protected override string ReadFile(string fullPath) => fullPath;
}
