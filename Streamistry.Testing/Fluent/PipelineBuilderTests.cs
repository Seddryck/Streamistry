﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Fluent;
using Streamistry.Testability;

namespace Streamistry.Testing.Fluent;
public class PipelineBuilderTests
{
    [Test]
    public void Build_EmptyPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder<int>().BuildPort();
        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_SourceWithinPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 2, 3])
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_SourceThenPipeWithinPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 2, 3])
            .Filter(x => x % 2 != 0)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_SourceThenSinkWithinPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 2, 3])
            .Sink().InMemory()
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_FilterCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 2, 3])
            .Filter(x => x % 2 != 0).Checkpoint(out var filter)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(filter, Is.Not.Null);

        var output = filter.GetOutputs(pipeline.Start);
        Assert.That(output, Has.Length.EqualTo(2));
    }

    [Test]
    public void Build_PluckerCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 2, 3])
            .Map(x => new DateOnly(2024, x, 1))
            .Pluck(x => x.Month).Checkpoint(out var plucker)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(plucker, Is.Not.Null);

        var output = plucker.GetOutputs(pipeline.Start);
        Assert.Multiple(() =>
        {
            Assert.That(output, Does.Contain(1));
            Assert.That(output, Does.Contain(2));
            Assert.That(output, Does.Contain(3));
        });
    }

    [Test]
    public void Build_SplitterCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["1-2-3", "4-5"])
            .Split(x => x?.Split('-') ?? []).Checkpoint(out var splitter)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(splitter, Is.Not.Null);

        var output = splitter.GetOutputs(pipeline.Start);
        Assert.Multiple(() =>
        {
            for (int i = 1; i <= 5; i++)
                Assert.That(output, Does.Contain(i.ToString()));
        });
    }

    [Test]
    public void Build_AggregateMaxCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 3, 2])
            .Aggregate().AsMax().Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(3));
    }

    [Test]
    public void Build_AggregateMinCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 3, 2])
            .Aggregate().AsMin().Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(1));
    }

    [Test]
    public void Build_AggregateAverageCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 3, 2])
            .Aggregate().AsAverage().Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(2));
    }

    [Test]
    public void Build_AggregateMedianCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 3, 2])
            .Aggregate().AsMedian().Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(2));
    }

    [Test]
    public void Build_AggregateSumCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 3, 2])
            .Aggregate().AsSum().Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(6));
    }

    [Test]
    public void Build_AggregateCountCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<int>()
            .Source([1, 3, 2])
            .Aggregate().AsCount().Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(3));
    }

    [Test]
    public void Build_AggregateUniversalCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["f", "oo", "Bar"])
            .Aggregate((x, y) => x + y).Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo("fooBar"));
    }

    [Test]
    public void Build_AggregateUniversal2Checkpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["f", "oo", "Bar"])
            .Aggregate<int>((x, y) => x + (string.IsNullOrEmpty(y) ? 0 : y!.Length)).Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(6));
    }


    [Test]
    public void Build_AggregateUniversal3Checkpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["f", "oo", "Bar"])
            .Aggregate<(int, int), bool>((x, y) => char.IsUpper(y![0]) ? (x.Item1++, x.Item2) : (x.Item1, x.Item2++))
                .WithSelector(x => x.Item1 > x.Item2)
                .Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(false));
    }

    [Test]
    public void Build_AggregateUniversal4Checkpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["f", "oo", "Bar"])
            .Aggregate<(int Upper, int Lower), bool>((x, y) => char.IsUpper(y![0]) ? (x.Upper++, x.Lower) : (x.Upper, x.Lower++))
                .WithSelector(x => x.Upper > x.Lower)
                .WithSeed((3, 0))
                .Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(true));
    }

    [Test]
    public void Build_ParserDateCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
                .Checkpoint(out var parser)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(parser, Is.Not.Null);

        var output = parser.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain(new DateOnly(2024, 09, 14)));
        Assert.That(output, Does.Contain(new DateOnly(2024, 09, 15)));
    }

    [Test]
    public void Build_ParserDateTimeCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["2024-09-14 11:12:20", "2024-09-15 17:12:16", "2024-45-78"])
            .Parse()
                .AsDateTime()
                .Checkpoint(out var parser)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(parser, Is.Not.Null);

        var output = parser.GetOutputs(pipeline.Start);
        Assert.That(output, Has.Length.EqualTo(2));
        Assert.That(output, Does.Contain(new DateTime(2024, 09, 14, 11, 12, 20)));
        Assert.That(output, Does.Contain(new DateTime(2024, 09, 15, 17, 12, 16)));
    }


    [Test]
    public void Build_ParserRomanFiguresCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<char>()
            .Source(['I', 'X', 'Z'])
            .Parse((char x, out int y) =>
            {
                y = x switch
                {
                    'I' => 1,
                    'V' => 5,
                    'X' => 10,
                    _ => -1
                };
                return y>=0;
            }).Checkpoint(out var parser)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(parser, Is.Not.Null);

        var output = parser.GetOutputs(pipeline.Start);
        Assert.That(output, Has.Length.EqualTo(2));
        Assert.That(output, Does.Contain(1));
        Assert.That(output, Does.Contain(10));
    }

    [Test]
    public void Build_ComplexTryOnlyMainCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder<string>()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Map(x => x.AddDays(1))
            .Pluck(x => x.Day)
            .Aggregate()
                .AsMax()
                .Checkpoint(out var aggr)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(aggr, Is.Not.Null);

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(16));
    }
}
