using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability.Measurements;
using Streamistry.Observability.Thresholds;

namespace Streamistry.Observability.Meters;
internal class Histogram<TInput> : BaseMeter
{
    protected Func<TInput, string> Bucketizer { get; }
    protected int CountItems { get; set; }
    protected Dictionary<string, int> Store { get; } = [];

    public Histogram(Func<TInput, string> bucketizer, IThreshold threshold, Action<IMeasurement> publisher)
        : this(bucketizer, [threshold], [publisher]) { }

    public Histogram(Func<TInput, string> bucketizer, IThreshold[] thresholds, Action<IMeasurement>[] publishers)
        : base(thresholds, publishers)
        => Bucketizer = bucketizer;

    protected override int Count()
        => CountItems;

    protected override void Update(object value, DateTime timestamp)
    {
        var bucket = Bucketizer.Invoke((TInput)value);
        if (Store.TryGetValue(bucket, out var count))
            Store[bucket] = ++count;
        else
            Store.Add(bucket, 1);
        CountItems += 1;
    }

    protected override void Reset()
    {
        foreach (var bucket in Store.Keys)
            Store[bucket] = 0;
        CountItems = 0;
    }

    protected override IMeasurement CreateMeasurement()
        => new HistogramMeasurement([.. Store]);
}
