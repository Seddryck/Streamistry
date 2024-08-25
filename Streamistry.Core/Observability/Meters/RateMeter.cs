using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability.Measurements;
using Streamistry.Observability.Thresholds;

namespace Streamistry.Observability.Meters;
internal class RateMeter : Counter
{
    private DateTime? StartWindow { get; set; }
    private DateTime? EndWindow { get; set; }

    public RateMeter(IThreshold threshold, Action<IMeasurement> publisher)
        : this([threshold], [publisher]) { }

    public RateMeter(IThreshold[] thresholds, Action<IMeasurement>[] publishers)
        : base(thresholds, publishers) { }

    protected override void Update(object value, DateTime timestamp)
    {
        base.Update(value, timestamp);
        StartWindow ??= timestamp;
        EndWindow = timestamp;
    }

    protected override void Reset()
    {
        base.Reset();
        StartWindow = null;
    }

    protected override IMeasurement CreateMeasurement()
        => new RateMeasurement(Value, StartWindow.HasValue && EndWindow.HasValue ? EndWindow.Value.Subtract(StartWindow.Value) : new TimeSpan(0));
}
