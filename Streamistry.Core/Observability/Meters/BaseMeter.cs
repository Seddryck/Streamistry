using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability.Measurements;
using Streamistry.Observability.Thresholds;

namespace Streamistry.Observability.Meters;
public abstract class BaseMeter : IMeter
{
    private Action<IMeasurement>? Publish { get; }
    private IThreshold[] Thresholds { get; }

    protected BaseMeter(IThreshold[] thresholds, Action<IMeasurement>[] publishers)
    {
        if (thresholds is null || thresholds.Length == 0)
            thresholds = [new MaxCardinality(1000)];
        Thresholds = thresholds;

        foreach (var publisher in publishers)
            Publish += publisher;
    }

    protected abstract int Count();
    protected abstract void Update(object value, DateTime timestamp);
    public void Append(object value, DateTime timestamp)
    {
        Update(value, timestamp);

        foreach (var threshold in Thresholds)
        {
            if (threshold.Check(Count(), timestamp))
            {
                Trigger();
                break;
            }
        }
    }

    protected abstract void Reset();
    protected abstract IMeasurement CreateMeasurement();
    public void Trigger()
    {
        Publish?.Invoke(CreateMeasurement());
        Reset();
    }
}
