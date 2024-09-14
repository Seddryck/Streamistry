using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
internal class PipelineBuilder<T> : IBuilder<Pipeline>
{
    protected Pipeline? Instance { get; set; }

    public SourceBuilder<T> Source(IEnumerable<T> enumeration)
        => new (this, enumeration);

    public Pipeline BuildPort()
        => Instance ??= OnBuildPort();

    public Pipeline OnBuildPort()
        => new Pipeline();
}
