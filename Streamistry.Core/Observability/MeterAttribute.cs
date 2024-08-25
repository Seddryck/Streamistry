using System.Diagnostics;
using System.Transactions;
using MethodBoundaryAspect.Fody.Attributes;
using Streamistry.Observability.Meters;

namespace Streamistry.Observability;

public sealed class MeterAttribute : OnMethodBoundaryAspect
{
    public override void OnEntry(MethodExecutionArgs args)
    {
        var observable = args.Instance as IObservablePipe ?? throw new InvalidOperationException();
        var meters = observable
            ?.GetObservabilityProvider()
            ?.GetMeters(observable);

        if (meters is not null && meters.Length>0)
        {
            var timestamp = DateTime.UtcNow;
            foreach (var meter in meters)
                meter.Append(args.Arguments[0], timestamp);
        }
    }

    public override void OnExit(MethodExecutionArgs args)
    { }

    public override void OnException(MethodExecutionArgs args)
    { }
}
