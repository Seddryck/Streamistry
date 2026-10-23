using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry.Pipes.Sources;
public class EnumerableSource<TOutput> : Source<TOutput>
{
    private readonly IEnumerable<TOutput> values;
    private IEnumerator<TOutput>? enumerator;
    public override void Setup() => enumerator = values.GetEnumerator();
    public override void Cleanup()
    {
        var current = enumerator;
        enumerator = null;
        current?.Dispose();
    }

    public EnumerableSource(IEnumerable<TOutput> values, ObservabilityProvider? provider = null)
        : base(provider)
        => this.values = values;

    public EnumerableSource(Pipeline upstream, IEnumerable<TOutput> values)
        : base(upstream)
        => this.values = values;

    protected override bool TryReadNext(out TOutput? item)
    {
        if (enumerator!.MoveNext())
        {
            item = enumerator.Current;
            return true;
        }
        item = default;
        return false;
    }
}
