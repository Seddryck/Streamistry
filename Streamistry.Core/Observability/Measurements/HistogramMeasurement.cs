using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability.Measurements;
public readonly record struct HistogramMeasurement(KeyValuePair<string, int>[] Buckets) : IMeasurement
{
    public override string ToString()
        => $"[{string.Join(", ", BucketsToString(Buckets))}]";

    private static IEnumerable<string> BucketsToString(KeyValuePair<string, int>[] buckets)
    {
        foreach (var bucket in buckets)
            yield return $"{{{bucket.Key} => {bucket.Value}}}";
    }
}
