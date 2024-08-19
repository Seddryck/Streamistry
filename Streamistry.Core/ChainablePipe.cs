﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public abstract class ChainablePipe<T> : IChainablePipe<T>
{
    private Action<T?>? Downstream { get; set; }

    public void RegisterDownstream(Action<T?> action)
        => Downstream += action;

    public void PushDownstream(T? obj)
        => Downstream?.Invoke(obj);
}
