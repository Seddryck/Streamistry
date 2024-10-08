﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
        var pipeline = new PipelineBuilder().BuildPipeElement();
        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_SourceWithinPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_SourceThenPipeWithinPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Filter(x => x % 2 != 0)
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_SourceThenSinkWithinPipeline_Pipeline()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Sink().InMemory()
            .Build();

        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Is.TypeOf<Pipeline>());
    }

    [Test]
    public void Build_FilterCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Filter(x => x % 2 != 0).Checkpoint(out var filter)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(filter, Is.Not.Null);
        });

        var output = filter.GetOutputs(pipeline.Start);
        Assert.That(output, Has.Length.EqualTo(2));
    }

    [Test]
    public void Build_PluckerCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Map(x => new DateOnly(2024, x, 1))
            .Pluck(x => x.Month).Checkpoint(out var plucker)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(plucker, Is.Not.Null);
        });

        var output = plucker.GetOutputs(pipeline.Start);
        Assert.Multiple(() =>
        {
            Assert.That(output, Does.Contain(1));
            Assert.That(output, Does.Contain(2));
            Assert.That(output, Does.Contain(3));
        });
    }

    [Test]
    public void Build_ConstantCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Constant(0).Checkpoint(out var constant)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(constant, Is.Not.Null);
        });

        var output = constant.GetOutputs(pipeline.Start);
        Assert.Multiple(() =>
        {
            Assert.That(output[0], Is.EqualTo(0));
            Assert.That(output[1], Is.EqualTo(0));
            Assert.That(output[2], Is.EqualTo(0));
        });
    }

    [Test]
    public void Build_SplitterCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["1-2-3", "4-5"])
            .Split(x => x?.Split('-') ?? []).Checkpoint(out var splitter)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(splitter, Is.Not.Null);
        });

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
        var pipeline = new PipelineBuilder()
            .Source([1, 3, 2])
            .Aggregate().AsMax().Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(3));
    }

    [Test]
    public void Build_AggregateMinCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 3, 2])
            .Aggregate().AsMin().Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(1));
    }

    [Test]
    public void Build_AggregateAverageCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 3, 2])
            .Aggregate().AsAverage().Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(2));
    }

    [Test]
    public void Build_AggregateMedianCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 3, 2])
            .Aggregate().AsMedian().Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(2));
    }

    [Test]
    public void Build_AggregateSumCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 3, 2])
            .Aggregate().AsSum().Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(6));
    }

    [Test]
    public void Build_AggregateCountCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 3, 2])
            .Aggregate().AsCount().Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(3));
    }

    [Test]
    public void Build_AggregateUniversalCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["f", "oo", "Bar"])
            .Aggregate((x, y) => x + y).Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo("fooBar"));
    }

    [Test]
    public void Build_AggregateUniversal2Checkpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["f", "oo", "Bar"])
            .Aggregate<int>((x, y) => x + (string.IsNullOrEmpty(y) ? 0 : y!.Length)).Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(6));
    }


    [Test]
    public void Build_AggregateUniversal3Checkpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["f", "oo", "Bar"])
            .Aggregate<(int, int), bool>((x, y) => char.IsUpper(y![0]) ? (x.Item1++, x.Item2) : (x.Item1, x.Item2++))
                .WithSelector(x => x.Item1 > x.Item2)
                .Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(false));
    }

    [Test]
    public void Build_AggregateUniversal4Checkpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["f", "oo", "Bar"])
            .Aggregate<(int Upper, int Lower), bool>((x, y) => char.IsUpper(y![0]) ? (x.Upper++, x.Lower) : (x.Upper, x.Lower++))
                .WithSelector(x => x.Upper > x.Lower)
                .WithSeed((3, 0))
                .Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(true));
    }

    [Test]
    public void Build_ParserDateCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
                .Checkpoint(out var parser)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(parser, Is.Not.Null);
        });

        var output = parser.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain(new DateOnly(2024, 09, 14)));
        Assert.That(output, Does.Contain(new DateOnly(2024, 09, 15)));
    }

    [Test]
    public void Build_ParserDateTimeCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14 11:12:20", "2024-09-15 17:12:16", "2024-45-78"])
            .Parse()
                .AsDateTime()
                .Checkpoint(out var parser)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(parser, Is.Not.Null);
        });

        var output = parser.GetOutputs(pipeline.Start);
        Assert.That(output, Has.Length.EqualTo(2));
        Assert.That(output, Does.Contain(new DateTime(2024, 09, 14, 11, 12, 20)));
        Assert.That(output, Does.Contain(new DateTime(2024, 09, 15, 17, 12, 16)));
    }


    [Test]
    public void Build_ParserRomanFiguresCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
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
                return y >= 0;
            }).Checkpoint(out var parser)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(parser, Is.Not.Null);
        });

        var output = parser.GetOutputs(pipeline.Start);
        Assert.That(output, Has.Length.EqualTo(2));
        Assert.That(output, Does.Contain(1));
        Assert.That(output, Does.Contain(10));
    }

    [Test]
    public void Build_ComplexTryOnlyMainCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Map(x => x.AddDays(1))
            .Pluck(x => x.Day)
            .Aggregate()
                .AsMax()
                .Checkpoint(out var aggr)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(aggr, Is.Not.Null);
        });

        var output = aggr.GetOutputs(pipeline.Start);
        Assert.That(output.Last(), Is.EqualTo(16));
    }


    [Test]
    public void Build_CombineTwoUpstreamsCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture)))
            .Zip((day, month) => $"{day} {month}").Checkpoint(out var zip)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(zip, Is.Not.Null);
        });

        var output = zip.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain("15 September"));
        Assert.That(output, Does.Contain("16 September"));
    }

    [Test]
    public void Build_CombineThreeUpstreamsCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
                , year => year.Pluck(x => x.Year).Map(x => x + 1))
            .Zip((day, month, year) => $"on {day} {month} {year}").Checkpoint(out var zip)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(zip, Is.Not.Null);
        });

        var output = zip.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain("on 15 September 2025"));
        Assert.That(output, Does.Contain("on 16 September 2025"));
    }

    [Test]
    public void Build_InBranchCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
            ).Checkpoints(out var ports)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(ports, Is.Not.Null);
        });
        Assert.That(ports, Has.Length.EqualTo(2));

        var outputMonth = ((IChainablePort<string>)ports[1]).GetOutputs(pipeline.Start);
        Assert.That(outputMonth, Does.Contain("September"));
    }

    [Test]
    public void Build_InBranchCheckpointForAllPorts_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
            ).Checkpoints(out var ports)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(ports, Is.Not.Null);
        });
        Assert.That(ports, Has.Length.EqualTo(2));

        var outputMonth = ((IChainablePort<string>)ports[1]).GetOutputs(pipeline.Start);
        Assert.That(outputMonth, Does.Contain("September"));
    }


    [Test]
    public void Build_InBranchCheckpointWithDiscardedPorts_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
                , year => year.Pluck(x => x.Year).Map(x => x + 1)
            ).Checkpoints(out var _, out var monthPort, out var _)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(monthPort, Is.Not.Null);
        });

        var outputMonth = monthPort.GetOutputs(pipeline.Start);
        Assert.That(outputMonth, Does.Contain("September"));
    }

    [Test]
    public void Build_InBranchCheckpointForAllPortsAllAsserted_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
            ).Checkpoints(out var ports)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(ports, Is.Not.Null);
        });
        Assert.That(ports, Has.Length.EqualTo(2));

        var action = pipeline.Start;
        var (outputDay, outputMonth) = action.GetMultipleOutputs((IChainablePort<int>)ports[0], (IChainablePort<string>)ports[1]);
        Assert.That(outputDay, Does.Contain(15));
        Assert.Multiple(() =>
        {
            Assert.That(outputDay, Does.Contain(16));
            Assert.That(outputMonth, Does.Contain("September"));
        });
    }

    [Test]
    public void Build_InBranchCheckpointForAllPortsTypedAllAsserted_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
            ).Checkpoints(out var portDay, out var portMonth)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(portDay, Is.Not.Null);
            Assert.That(portMonth, Is.Not.Null);
        });

        var action = pipeline.Start;
        var (outputDay, outputMonth) = action.GetMultipleOutputs(portDay, portMonth);
        Assert.That(outputDay, Does.Contain(15));
        Assert.Multiple(() =>
        {
            Assert.That(outputDay, Does.Contain(16));
            Assert.That(outputMonth, Does.Contain("September"));
        });
    }

    [Test]
    public void Build_InBranchOfBranchCheckpointForAllPortsTypedAllAsserted_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-09-14", "2024-09-15", "2024-45-78"])
            .Parse()
                .AsDate()
            .Branch(
                day => day.Map(x => x.AddDays(1)).Pluck(x => x.Day).Branch(
                    day1 => day1.Map(x => x + 1)
                    , day2 => day2.Map(x => x + 2)
                ).Zip((x, y) => x + y)
                , month => month.Map(x => x.ToString("MMMM", CultureInfo.InvariantCulture))
            ).Checkpoints(out var portDay, out var portMonth)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(portDay, Is.Not.Null);
            Assert.That(portMonth, Is.Not.Null);
        });

        var action = pipeline.Start;
        var (outputDay, outputMonth) = action.GetMultipleOutputs(portDay, portMonth);
        Assert.That(outputDay, Does.Contain(33));
        Assert.Multiple(() =>
        {
            Assert.That(outputDay, Does.Contain(35));
            Assert.That(outputMonth, Does.Contain("September"));
        });
    }

    [Test]
    public void Build_CombineFiveUpstreamsCheckpoint_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Branch(
                stream1 => stream1.Map(x => x += 1)
                , stream2 => stream2.Map(x => x += 2)
                , stream3 => stream3.Map(x => x += 3)
                , stream4 => stream4.Map(x => x += 4)
                , stream5 => stream5.Map(x => x += 5))
            .Zip((stream1, stream2, stream3, stream4, stream5) => stream1 + stream2 + stream3 + stream4 + stream5).Checkpoint(out var zip)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(zip, Is.Not.Null);
        });

        var output = zip.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain(20));
        Assert.That(output, Does.Contain(25));
        Assert.That(output, Does.Contain(30));
    }

    [Test]
    public void Build_WithSingleSegment_Success()
    {
        var segment = new Segment<int, int>(x => x.Map(y => y * 2).Filter(y => y % 5 == 3));

        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Map(x => x * 3)
            .Bind(segment)
            .Map(x => x % 4).Checkpoint(out var sink)
            .Build();

        var output = sink.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain(2));
    }

    [Test]
    public void Build_WithSingleSegmentChangeType_Success()
    {
        var segment = new Segment<int, string>(x => x.Map(y => y + 3).Map(y => new string('*', y)));

        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3])
            .Map(x => x - 2)
            .Bind(segment)
            .Map(x => x!.PadLeft(4, '-')).Checkpoint(out var sink)
            .Build();

        var output = sink.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain("--**"));
        Assert.That(output, Does.Contain("-***"));
        Assert.That(output, Does.Contain("****"));
    }

    [Test]
    public void Build_WithManySegments_Success()
    {
        var odd = new Segment<int, int>(x => x.Filter(y => y % 2 == 1).Map(y => y * 2));
        var even = new Segment<int, int>(x => x.Filter(y => y % 2 == 0).Map(y => y * 5));

        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3, 6])
            .Branch(odd, even)
            .Zip((x, y) => x + y).Checkpoint(out var zip)
            .Build();

        var output = zip.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain(12));
        Assert.That(output, Does.Contain(36));
    }

    [Test]
    public void Build_WithUnion_Success()
    {
        var odd = new Segment<int, int>(x => x.Filter(y => y % 2 == 1).Map(y => y * 2));
        var even = new Segment<int, int>(x => x.Filter(y => y % 2 == 0).Map(y => y * 5));

        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3, 6])
            .Branch(odd, even)
            .Union().Checkpoint(out var union)
            .Build();

        var output = union.GetOutputs(pipeline.Start);
        Assert.That(output, Does.Contain(2));
        Assert.That(output, Does.Contain(10));
        Assert.That(output, Does.Contain(6));
        Assert.That(output, Does.Contain(30));
    }

    [Test]
    public void Build_WithUnionButDifferentType_Failure()
    {
        var odd = new Segment<int, int>(x => x.Filter(y => y % 2 == 1).Map(y => y * 2));
        var even = new Segment<int, string>(x => x.Filter(y => y % 2 == 0).Map(y => new string('*', y)));

        var pipeline = new PipelineBuilder()
            .Source([1, 2, 3, 6])
            .Branch(odd, even);

        var ex = Assert.Throws<InvalidUpstreamBranchException>(() => pipeline.Union());
        Assert.That(ex.Message, Is.EqualTo("Input branches of the Union operators must have the same type. Following distinct types were detected 'Int32' and 'String'"));
    }

    public record Animal(string Name);
    public record Carnivore(string Name) : Animal(Name) { public string? Eat { get; set; } }
    public record Frugivore(string Name) : Animal(Name) { public string? Eat { get; set; } }

    [Test]
    public void Build_WithUnionButDifferentTypeLinkedByInheritance_Failure()
    {
        var notCarnivore = new Segment<Animal, Animal>(x => x.Filter(y => y is not Carnivore).Map(y => y));
        var carnivore = new Segment<Animal, Carnivore>(x => x.Filter(y => y is Carnivore).Map(y => (Carnivore)y!));

        var pipeline = new PipelineBuilder()
            .Source([new Animal("Bird"), new Carnivore("Dog")])
            .Branch(notCarnivore, carnivore);

        var ex = Assert.Throws<InvalidUpstreamBranchException>(() => pipeline.Union());
        Assert.That(ex.Message, Is.EqualTo("Input branches of the Union operators must have the same type. Following distinct types were detected 'Animal' and 'Carnivore'"));
    }

    [Test]
    public void Build_WithMoreThanTwoUpstreamsUnion_Success()
    {
        var common = new Segment<Animal, Animal>(x => x.Filter(y => y is not Carnivore && y is not Frugivore).Map(y => y));
        var carnivore = new Segment<Animal, Animal>(x => x.Cast<Carnivore>().Safe().Map(y => { y!.Eat = "Meat"; return y; }).Cast<Animal>());
        var frugivore = new Segment<Animal, Animal>(x => x.Cast<Frugivore>().Safe().Map(y => { y!.Eat = "Fruit"; return y; }).Cast<Animal>());

        var pipeline = new PipelineBuilder()
            .Source([new Animal("Bird"), new Carnivore("Dog")])
            .Branch(common, carnivore, frugivore)
            .Union().Checkpoint(out var union)
            .Build();

        Assert.That(union.GetOutputs(pipeline.Start), Has.Length.EqualTo(2));
    }

    [Test]
    public void Build_WithNullAndNotNull_Success()
    {
        var isNull = new Segment<Animal?, Animal>(x => x.IsNull().Constant(new Animal("Dragon")));
        var isNotNull = new Segment<Animal?, Animal>(x => x.IsNotNull<Animal>().Cast<Carnivore>().Safe().Map(y => { y!.Eat = "Meat"; return y; }).Cast<Animal>());

        var pipeline = new PipelineBuilder()
            .Source([new Animal("Bird"), null, new Carnivore("Dog"), new Frugivore("Parrot")])
            .Branch(isNull, isNotNull)
            .Union().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(2));
        Assert.That(outputs.Select(x => x!.Name), Does.Contain("Dragon"));
        Assert.That(outputs.Select(x => x!.Name), Does.Contain("Dog"));
    }

    [Test]
    public void Build_WithNullAndNotNullValueType_Success()
    {
        var isNull = new Segment<int?, int>(x => x.IsNull().Constant(0));
        var isNotNull = new Segment<int?, int>(x => x.IsNotNull<int>());

        var pipeline = new PipelineBuilder()
            .Source(new int?[] { 1, null, 3 })
            .Branch(isNull, isNotNull)
            .Union().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Does.Contain(1));
        Assert.That(outputs, Does.Contain(0));
        Assert.That(outputs, Does.Contain(3));
    }

    [Test]
    public void Build_WithRoutes_Success()
    {
        var main = new Segment<int, int>(x => x.Map(x => ++x));
        var exception = new Segment<int, int>(x => x.Map(x => --x));

        var pipeline = new PipelineBuilder()
            .Source([0, 1, 2])
            .Map(x => 6 / x).Safe()
                .Route(x => x.Main, main)
                .Route(x => x.Alternate, exception)
            .Union<int>().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Does.Contain(7));
        Assert.That(outputs, Does.Contain(4));
        Assert.That(outputs, Does.Contain(-1));
    }

    [Test]
    public void Build_WithRoutesInline_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([0, 1, 2])
            .Map(x => 6 / x).Safe()
                .Route<int, int>(x => x.Main, x => x.Map(x => ++x))
                .Route<int, int>(x => x.Alternate, x => x.Map(x => --x))
            .Union<int>().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Does.Contain(7));
        Assert.That(outputs, Does.Contain(4));
        Assert.That(outputs, Does.Contain(-1));
    }

    [Test]
    public void Build_WithRoutesInlineConverge_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source([0, 1, 2])
            .Map(x => 6 / x).Safe()
            .Converge<int>()
                .Route<int>(x => x.Main, x => x.Map(x => ++x))
                .Route<int>(x => x.Alternate, x => x.Map(x => --x))
            .Union().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Does.Contain(7));
        Assert.That(outputs, Does.Contain(4));
        Assert.That(outputs, Does.Contain(-1));
    }


    [Test]
    public void Build_WithRoutesInlineForParsers_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-02-16", "2024-02-58", "2024-02-20"])
            .Parse()
                .AsDate()
                    .Route<DateOnly, DateOnly>(x => x.Main, x => x.Map(x => x.AddDays(-1)))
                    .Route<string, DateOnly>(x => x.Alternate, x => x.Constant(new DateOnly(2024, 2, 1)))
            .Union<DateOnly>().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Does.Contain(new DateOnly(2024, 2, 15)));
        Assert.That(outputs, Does.Contain(new DateOnly(2024, 2, 19)));
        Assert.That(outputs, Does.Contain(new DateOnly(2024, 2, 1)));
    }

    [Test]
    public void Build_WithRoutesForParsers_Success()
    {
        var pipeline = new PipelineBuilder()
            .Source(["2024-02-16", "2024-02-58", "2024-02-20"])
            .Parse()
                .AsDate()
            .Converge<DateOnly>()
                .Route<DateOnly>(x => x.Main, x => x.Map(x => x.AddDays(-1)))
                .Route<string>(x => x.Alternate, x => x.Constant(new DateOnly(2024, 2, 1)))
            .Union().Checkpoint(out var union)
            .Build();

        var outputs = union.GetOutputs(pipeline.Start);
        Assert.That(outputs, Has.Length.EqualTo(3));
        Assert.That(outputs, Does.Contain(new DateOnly(2024, 2, 15)));
        Assert.That(outputs, Does.Contain(new DateOnly(2024, 2, 19)));
        Assert.That(outputs, Does.Contain(new DateOnly(2024, 2, 1)));
    }
}
