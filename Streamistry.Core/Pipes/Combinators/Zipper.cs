﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public class Zipper<TFirst, TSecond, TResult> : Combinator<TFirst, TSecond, TResult>
{
    private Queue<TFirst?> FirstQueue { get; } = new();
    private Queue<TSecond?> SecondQueue { get; } = new();

    public Zipper(IChainablePipe<TFirst> firstUpstream, IChainablePipe<TSecond> secondUpstream, Func<TFirst?, TSecond?, TResult?> function)
        : base(firstUpstream, secondUpstream, function)
    { }

    protected override bool TryGetElement<T>(out T? value) where T : default
    {
        if (typeof(T) == typeof(TFirst) && FirstQueue.TryDequeue(out var first))
        {
            value = first is T t ? t : throw new InvalidProgramException();
            return true;
        }
        else if (typeof(T) == typeof(TSecond) && SecondQueue.TryDequeue(out var second))
        {
            value = second is T t ? t : throw new InvalidProgramException();
            return true;
        }
        value = default;
        return false;
    }

    protected override void Queue<T>(T value)
    {
        if (value is TFirst first)
            FirstQueue.Enqueue(first);
        else if (value is TSecond second)
            SecondQueue.Enqueue(second);
        else throw new ArgumentOutOfRangeException(nameof(value));
    }
}