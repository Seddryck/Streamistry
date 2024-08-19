﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Streamistry.Pipes.Sinks;
public class DebugOutputSink<T> : Sink<T>
{
    public DebugOutputSink(IChainablePipe<T> upstream)
        : base(upstream, Process)
    { }

    public static void Process(T? obj)
        => Console.WriteLine(obj);
}
