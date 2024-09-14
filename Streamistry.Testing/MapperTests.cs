using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Sinks;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class MapperTests
{
    [Test]
    public void Emit_MapperWithoutSink_Successful()
    {
        int result = 0;

        var mapper = new Mapper<int, int>(x => ++x);
        mapper.RegisterDownstream(x => result = x);

        Assert.That(() => { mapper.Emit(0); return result; }, Is.EqualTo(1));
        Assert.That(() => { mapper.Emit(1); return result; }, Is.EqualTo(2));
        Assert.That(() => { mapper.Emit(10); return result; }, Is.EqualTo(11));
    }

    [Test]
    public void Emit_MapperWithExtensionMethod_Successful()
    {
        var mapper = new Mapper<int, int>(x => ++x);

        Assert.That(mapper.EmitAndGetOutput(0), Is.EqualTo(1));
        Assert.That(mapper.EmitAndGetOutput(1), Is.EqualTo(2));
        Assert.That(mapper.EmitAndGetOutput(10), Is.EqualTo(11));
    }

    [Test]
    public void Emit_MapperWithSink_Successful()
    {
        var mapper = new Mapper<int, int>(x => ++x);
        var sink = new MemorySink<int>(mapper);
        mapper.Emit(0);

        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.First(), Is.EqualTo(1));
    }


    [Test]
    public void Mapper_MapperWithSinkAndState_Successful()
    {
        var mapper = new Mapper<int, int>(x => ++x);
        var sink = new MemorySink<int>(mapper);

        mapper.Emit(0);
        Assert.That(sink.State, Has.Count.EqualTo(1));
        Assert.That(sink.State.Last(), Is.EqualTo(1));

        mapper.Emit(50);
        Assert.That(sink.State, Has.Count.EqualTo(2));
        Assert.That(sink.State.Last(), Is.EqualTo(51));

        mapper.Emit(3);
        Assert.That(sink.State, Has.Count.EqualTo(3));
        Assert.That(sink.State.Last(), Is.EqualTo(4));
    }

    [Test]
    public void Emit_MapperLengthSupportingNull_Successful()
    {
        var mapper = new Mapper<string?, int>(x => x?.Length ?? 0);

        Assert.Multiple(() =>
        {
            Assert.That(mapper.EmitAndGetOutput("Hello"), Is.EqualTo(5));
            Assert.That(mapper.EmitAndGetOutput("World"), Is.EqualTo(5));
            Assert.That(mapper.EmitAndGetOutput("!"), Is.EqualTo(1));
            Assert.That(mapper.EmitAndGetOutput(string.Empty), Is.EqualTo(0));
            Assert.That(mapper.EmitAndGetOutput(null), Is.EqualTo(0));
        });
    }

    [Test]
    public void Emit_ChainMappers_Successful()
    {
        var length = new Mapper<string, int>(x => x?.Length ?? 0);
        var asterisk = new Mapper<int, string>(length, x => new string('*', x));

        Assert.Multiple(() =>
        {
            Assert.That(length.EmitAndGetOutput("Hello World!", asterisk), Is.EqualTo("************"));
            Assert.That(length.EmitAndGetOutput("", asterisk), Is.EqualTo(string.Empty));
            Assert.That(length.EmitAndGetOutput("!", asterisk), Is.EqualTo("*"));
        });
    }

    [Test]
    public void Emit_ChainMappersFromPipeline_Successful()
    {
        var length = new Mapper<string, int>(x => x?.Length ?? 0);
        var asterisk = new Mapper<int, string>(length, x => new string('*', x));

        Assert.Multiple(() =>
        {
            Assert.That(length.EmitAndGetOutput("Hello World!", asterisk), Is.EqualTo("************"));
            Assert.That(length.EmitAndGetOutput("", asterisk), Is.EqualTo(string.Empty));
            Assert.That(length.EmitAndGetOutput("!", asterisk), Is.EqualTo("*"));
        });
    }

    [Test]
    public void Emit_ChainMappersBothAsserted_Successful()
    {
        var length = new Mapper<string, int>(x => x?.Length ?? 0);
        var asterisk = new Mapper<int, string>(length, x => new string('*', x));

        Assert.Multiple(() =>
        {
            Assert.That(length.EmitAndGetOutput("Hello World!"), Is.EqualTo(12));
            Assert.That(length.EmitAndGetOutput(""), Is.EqualTo(0));
            Assert.That(length.EmitAndGetOutput("!"), Is.EqualTo(1));
        });

        Assert.Multiple(() =>
        {
            Assert.That(length.EmitAndGetOutput("Hello World!", asterisk), Is.EqualTo("************"));
            Assert.That(length.EmitAndGetOutput("", asterisk), Is.EqualTo(string.Empty));
            Assert.That(length.EmitAndGetOutput("!", asterisk), Is.EqualTo("*"));
        });
    }

    [Test]
    public void Emit_InlineMappersWithSink_Successful()
    {
        var source = new EmptySource<string>();
        var length = new Mapper<string, int>(source, x => x?.Length ?? 0);
        var asterisk = new Mapper<string, string>(source, x => $"***{x}***");
        var lengthSink = new MemorySink<int>(length);
        var asteriskSink = new MemorySink<string>(asterisk);

        source.Emit("Hello world!");
        Assert.Multiple(() =>
        {
            Assert.That(lengthSink.State, Has.Count.EqualTo(1));
            Assert.That(lengthSink.State.Last(), Is.EqualTo(12));
            Assert.That(asteriskSink.State, Has.Count.EqualTo(1));
            Assert.That(asteriskSink.State.Last(), Is.EqualTo("***Hello world!***"));
        });
    }

    [Test]
    public void Emit_InlineMappersWithExtensions_Successful()
    {
        var source = new EmptySource<string>();
        var length = new Mapper<string, int>(source, x => x?.Length ?? 0);
        var asterisk = new Mapper<string, string>(source, x => $"***{x}***");
        
        Assert.Multiple(() =>
        {
            var results = source.EmitAndGetOutputs("Hello world!", [length, asterisk]);
            Assert.That(results, Has.Length.EqualTo(2));
            Assert.That(results[0], Is.EqualTo(12));
            Assert.That(results[1], Is.EqualTo("***Hello world!***"));
        });
    }
}
