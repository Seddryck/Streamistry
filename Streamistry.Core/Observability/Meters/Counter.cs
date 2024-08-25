using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability.Measurements;
using Streamistry.Observability.Thresholds;

namespace Streamistry.Observability.Meters;
internal class Counter : BaseMeter
{
    protected int Value { get; set; }
    
    public Counter(IThreshold threshold, Action<IMeasurement> publisher)
        : this([threshold], [publisher]) { }

    public Counter(IThreshold[] thresholds, Action<IMeasurement>[] publishers)
        :base(thresholds, publishers) { }

    protected override void Update(object value, DateTime timestamp)
        => Value += 1;

    protected override void Reset()
        => Value = 0;
    protected override int Count()
        => Value;
    protected override IMeasurement CreateMeasurement()
        => new CountMeasurement(Value);
}
