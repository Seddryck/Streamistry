using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability;

namespace Streamistry.Pipes.Mappers;
public class Translator<TInput, TOutput> : Mapper<TInput, TOutput>, IPreparablePipe where TInput : notnull
{
    private Dictionary<TInput, TOutput> Store { get; } = [];
    private Action? Preparation { get; set; }

    public Translator(IChainablePipe<TInput> upstream
                        , IChainablePipe<KeyValuePair<TInput, TOutput>> dictionary)
        : this(upstream, dictionary, x => default!)
    { }

    public Translator(IChainablePipe<TInput> upstream
                        , IChainablePipe<KeyValuePair<TInput, TOutput>> dictionary
                        , Func<TInput, TOutput> notFound)
        : base(upstream, notFound)
    {
        dictionary.RegisterDownstream(EmitDictionayEntry, CompleteDictionayEntry);
        if (upstream is Source<TInput> source)
            Preparation += source.Start;
    }

    [Meter]
    public void EmitDictionayEntry(KeyValuePair<TInput, TOutput> kv)
    {
        if (!Store.TryAdd(kv.Key, kv.Value))
            Store[kv.Key] = kv.Value;
    }

    public virtual void CompleteDictionayEntry()
        => Preparation?.Invoke();

    protected override TOutput Invoke(TInput value)
    {
        if (value is not null && Store.TryGetValue(value, out var output))
            return output;
        else
            return base.Invoke(value!);
    }

    public void RegisterOnPrepared(Action downstream)
        => Preparation += downstream;
}
