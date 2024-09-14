using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Streamistry.Pipes.Sinks;
public class DebugOutputSink<T> : Sink<T>
{
    public DebugOutputSink()
        : base(Process, null)
    { }

    public DebugOutputSink(IChainablePipe<T> upstream)
        : base(Process, upstream)
    { }

    public static void Process(T? obj)
    {
        Console.Write(">>> ");
        Console.WriteLine(obj);
    }
}
