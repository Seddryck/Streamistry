﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Sinks;
public class MemorySink<T> : Sink<T>
{
    public IList<T?> State { get; }

    public MemorySink(IChainablePort<T> upstream)
        : this(upstream, []) { }

    private MemorySink(IChainablePort<T> upstream, IList<T?> state)
        : base(upstream, state.Add)
        => State = state;
}
