using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;
public interface IObservablePipe
{
    void RegisterObservability(ObservabilityProvider? provider);
    ObservabilityProvider? GetObservabilityProvider();
}
