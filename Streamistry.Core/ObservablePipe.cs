using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public abstract class ObservablePipe : IObservablePipe
{
    protected ObservabilityProvider? Observability { get; private set; }

    protected ObservablePipe(ObservabilityProvider? observability)
        => Observability = observability;

    public void RegisterObservability(ObservabilityProvider? observability)
        => Observability = observability;

    public ObservabilityProvider? GetObservabilityProvider()
        => Observability;
}
