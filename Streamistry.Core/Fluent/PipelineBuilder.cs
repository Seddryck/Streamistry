using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public class PipelineBuilder : IBuilder<Pipeline>
{
    protected Pipeline? Instance { get; set; }

    public SourceBuilder<T> Source<T>(IEnumerable<T> enumeration)
        => new (this, enumeration);

    public Pipeline BuildPipeElement()
        => Instance ??= OnBuildPipeElement();

    public Pipeline OnBuildPipeElement()
        => new();
}
