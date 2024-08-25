﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

/// <summary>
/// Represents a pipeline element that merges two upstream streams into a single downstream stream by applying a specified mapping function to corresponding values from each upstream.
/// The output stream type is determined by the result of the mapping function applied to the input elements.
/// </summary>
/// <typeparam name="TFirst">The type of the elements in the first input stream.</typeparam>
/// <typeparam name="TSecond">The type of the elements in the second input stream.</typeparam>
/// <typeparam name="TResult">The type of the elements in the output stream, determined by the result of the mapping function.</typeparam>
public abstract class Combinator<TFirst, TSecond, TResult> : ChainablePipe<TResult>
{
    public Func<TFirst?, TSecond?, TResult?> Function { get; init; }
    protected int BranchesCompleted { get; set; }

    public Combinator(IChainablePipe<TFirst> firstUpstream, IChainablePipe<TSecond> secondUpstream, Func<TFirst?, TSecond?, TResult?> function)
    : base(firstUpstream.GetObservabilityProvider())
    {
        firstUpstream.RegisterDownstream(EmitFirst, Complete);
        secondUpstream.RegisterDownstream(EmitSecond, Complete);

        Function = function;
    }

    public void EmitFirst(TFirst? first)
    {
        if (TryGetElement<TSecond>(out var second))
            PushDownstream(Invoke(first, second));
        else
            Queue(first);
    }

    public void EmitSecond(TSecond? second)
    {
        if (TryGetElement<TFirst>(out var first))
            PushDownstream(Invoke(first, second));
        else
            Queue(second);
    }

    public override void Complete()
    {
        BranchesCompleted += 1;
        if (BranchesCompleted > 1)
        {
            BranchesCompleted = 0;
            PushComplete();
        }
    }

    [Trace]
    protected TResult? Invoke(TFirst? first, TSecond? second)
        => Function.Invoke(first, second);

    protected abstract bool TryGetElement<T>(out T? value);
    protected abstract void Queue<T>(T value);
}
