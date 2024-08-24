using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry.Pipes.Sources;
public class EnumerableSource<TOutput> : Source<TOutput>
{
    private IEnumerator<TOutput> Enumerator { get; set; }

    public EnumerableSource(IEnumerable<TOutput> values, ObservabilityProvider? provider = null)
        : base(provider)
        => Enumerator = values.GetEnumerator();

    protected override bool TryReadNext(out TOutput? item)
    {
        if (Enumerator.MoveNext())
        {
            item = Enumerator.Current;
            return true;
        }
        item = default;
        return false;
    }
}
