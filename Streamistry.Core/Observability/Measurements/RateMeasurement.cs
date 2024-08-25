using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability.Measurements;
public readonly record struct RateMeasurement(int Value, TimeSpan window) : IMeasurement
{
    public override string ToString()
        => $"{Math.Round(RatePerSecond, 3)} batch/sec";

    public double RatePerSecond
        => window.Ticks > 0 ? (Value * 1e7 / window.Ticks ) : double.NaN;
}
