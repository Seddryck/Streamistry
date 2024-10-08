﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;

public abstract partial class BasePipeBuilder<TOutput> : IPipeBuilder<TOutput>
{
    protected IChainablePort<TOutput>? Instance { get; set; }

    public abstract IChainablePort<TOutput> OnBuildPipeElement();

    public IChainablePort<TOutput> BuildPipeElement()
        => Instance ??= OnBuildPipeElement();

    public Pipeline Build()
    {
        BuildPipeElement();
        return Instance!.Pipe.Pipeline!;
    }

    public BasePipeBuilder<TOutput> Checkpoint(out IChainablePort<TOutput> port)
    {
        port = BuildPipeElement();
        return this;
    }

    public SinkBuilder<TOutput> Sink()
        => new(this);

    public MapperBuilder<TOutput, TNext> Map<TNext>(Func<TOutput, TNext>? function)
        => new(this, function);
    public FilterBuilder<TOutput> Filter(Func<TOutput, bool>? function)
        => new(this, function);
    public FilterNullBuilder<TOutput> IsNull()
        => new(this);
    public FilterNotNullBuilder<TOutput, TNext> IsNotNull<TNext>()
        where TNext : notnull
        => new(this);
    public PluckerBuilder<TOutput, TNext> Pluck<TNext>(Expression<Func<TOutput, TNext>> expr)
        => new(this, expr);
    public CasterBuilder<TOutput, TNext> Cast<TNext>()
        => new(this);
    public ConstantBuilder<TOutput, TNext> Constant<TNext>(TNext value)
        => new(this, value);
    public SplitterBuilder<TOutput, TNext> Split<TNext>(Func<TOutput, TNext[]>? function)
        => new(this, function);

    public UniversalAggregatorBuilder<TOutput, TAccumulate, TNext> Aggregate<TAccumulate, TNext>(Func<TAccumulate, TOutput, TAccumulate> accumulator)
        => new(this, accumulator);
    public UniversalAggregatorBuilder<TOutput, TNext, TNext> Aggregate<TNext>(Func<TNext, TOutput, TNext> accumulator)
        => new(this, accumulator);
    public UniversalAggregatorBuilder<TOutput, TOutput, TOutput> Aggregate(Func<TOutput, TOutput, TOutput> accumulator)
        => new(this, accumulator);
    public AggregatorBuilder<TOutput, TOutput, TOutput> Aggregate()
        => new(this);

    public ParserBuilder<TOutput, TNext> Parse<TNext>(ParserDelegate<TOutput, TNext> parser)
        => new(this, parser);
    public ParserBuilder<TOutput> Parse()
        => new(this);

    public BinderBuilder<TOutput, TNext> Bind<TNext>(Segment<TOutput, TNext> segment)
        => new(this, segment);
}

