using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability.Measurements;
public readonly record struct CountMeasurement(int Value) : IMeasurement
{
    public override string ToString()
        => $"{Value} batches";
}
