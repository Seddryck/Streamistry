﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry;

public interface IParser<TInput, TOutput> : IChainablePipe<TOutput>, IProcessablePipe<TInput>
{ }

public delegate bool ParserDelegate<TInput, TOutput>(TInput? input, out TOutput? value);

/// <summary>
/// Represents a pipeline element that parses each element within a batch of this stream according to a specified set of rules or grammar.
/// The output stream is determined by the structured format resulting from the parsing process applied to the input elements.
/// </summary>
/// <typeparam name="TInput">The type of the elements in the input stream before parsing.</typeparam>
/// <typeparam name="TOutput">The type of the elements in the output stream after parsing, typically a structured representation of the input.</typeparam>
public abstract class Parser<TInput, TOutput> : ChainablePipe<TOutput>, IParser<TInput, TOutput>, IDualRoute<TOutput, TInput>
{
    public ParserDelegate<TInput, TOutput> ParseFunction { get; init; }
    public OutputPort<TInput> Alternate { get; }
    public new OutputPort<TOutput> Main { get => base.Main; }

    public Parser(IChainablePort<TInput> upstream, ParserDelegate<TInput, TOutput> parseFunction)
    : base(upstream.Pipe.GetObservabilityProvider())
    {
        Alternate = new(this, "Alternate");
        upstream.RegisterDownstream(Emit);
        upstream.Pipe.RegisterOnCompleted(Complete);
        ParseFunction = parseFunction;
    }

    [Meter]
    public void Emit(TInput? obj)
    {
        if (TryInvoke(obj, out var value))
            PushDownstream(value);
        else
            Alternate.PushDownstream(obj);
    }

    [Trace]
    protected virtual bool TryInvoke(TInput? obj, out TOutput? value)
        => ParseFunction.Invoke(obj, out value);
}

public interface IStringParser<TOutput> : IParser<string, TOutput>
{ }

public abstract class StringParser<TOutput> : Parser<string, TOutput>, IStringParser<TOutput>
{
    public StringParser(IChainablePipe<string> upstream, ParserDelegate<string, TOutput> parseFunction)
    : base(upstream, parseFunction)
    { }
}