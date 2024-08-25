using System.Diagnostics;
using System.Transactions;
using MethodBoundaryAspect.Fody.Attributes;

namespace Streamistry.Observability;

public sealed class TraceAttribute : OnMethodBoundaryAspect
{
    public override void OnEntry(MethodExecutionArgs args)
    {
        args.MethodExecutionTag = (args.Instance as IObservablePipe)
            ?.GetObservabilityProvider()
            ?.GetTracer()
            .StartActiveSpan(args.Instance.GetType().Name.Split('`')[0]);
    }

    public override void OnExit(MethodExecutionArgs args)
    {
        var span = (IDisposable?)args.MethodExecutionTag;
        span?.Dispose();
    }

    public override void OnException(MethodExecutionArgs args)
    {
        var span = (IDisposable?)args.MethodExecutionTag;
        span?.Dispose();
    }
}
