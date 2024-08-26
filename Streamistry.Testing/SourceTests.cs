using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Mappers;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;

namespace Streamistry.Testing;
public class SourceTests
{
    [Test]
    public void Start_PipelineWithTwoSources_BothStarted()
    {
        var firstSource = new EnumerableSource<int>([0, 1, 0, 1]);
        var secondSource = new EnumerableSource<int>([0, 0, 1, 1]);

        var pipeline = new Pipeline([firstSource, secondSource]);
        var zipper = new Zipper<int, int, int>(firstSource, secondSource, (x, y) => x + y);
        var sink = new MemorySink<int>(zipper);
        pipeline.Start();

        Assert.That(sink.State, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(sink.State.Count(x => x == 0), Is.EqualTo(1));
            Assert.That(sink.State.Count(x => x == 1), Is.EqualTo(2));
            Assert.That(sink.State.Count(x => x == 2), Is.EqualTo(1));
        });
    }


    [Test]
    public void OnReady_Translator_SourceAutomaticallyRegistered()
    {
        var dictSource = new EnumerableSource<KeyValuePair<int, string>>
                        (
                            [new KeyValuePair<int, string>(0, "foo")
                            , new KeyValuePair<int, string>(1, "bar")]
                        );
        var valueSource = new EnumerableSource<int>([0, 1, 0, 0, 1, 0]);

        var pipeline = new Pipeline(dictSource);
        var translator = new Translator<int, string>(valueSource, dictSource, x => "quark");
        var sink = new MemorySink<string>(translator);
        pipeline.Start();

        Assert.That(sink.State.Count(x => x == "foo"), Is.EqualTo(4));
        Assert.That(sink.State.Count(x => x == "bar"), Is.EqualTo(2));
    }

    [Test]
    public void OnReady_Translator_SourceStartingAfterPreparation()
    {
        var dictSource = new EnumerableSource<KeyValuePair<int, string>>
                        (
                            [new KeyValuePair<int, string>(0, "foo")
                            , new KeyValuePair<int, string>(1, "bar")]
                        );
        var valueSource = new EnumerableSource<int>([0, 1, 0, 0, 1, 0]);

        var pipeline = new Pipeline(dictSource);
        var increment = new Mapper<int, int>(valueSource, x => x += 1);
        var translator = new Translator<int, string>(increment, dictSource, x => "quark");
        valueSource.WaitOnPrepared(translator);
        var sink = new MemorySink<string>(translator);
        pipeline.Start();

        Assert.That(sink.State, Has.Count.EqualTo(6));
        Assert.That(sink.State.Count(x => x == "bar"), Is.EqualTo(4));
        Assert.That(sink.State.Count(x => x == "quark"), Is.EqualTo(2));
    }

    [Test]
    public void OnReady_TranslatorWithTwoSources_SourcesStartingAfterPreparation()
    {
        var dictSource = new EnumerableSource<KeyValuePair<int, string>>
                        (
                            [new KeyValuePair<int, string>(0, "foo")
                            , new KeyValuePair<int, string>(1, "bar")]
                        );
        var firstSource = new EnumerableSource<int>([0, 1, 0, 1]);
        var secondSource = new EnumerableSource<int>([0, 0, 1, 1]);

        var pipeline = new Pipeline(dictSource);
        var sum = new Zipper<int, int, int>(firstSource, secondSource, (x, y) => x + y);
        var translator = new Translator<int, string>(sum, dictSource, x => "quark");
        firstSource.WaitOnPrepared(translator);
        secondSource.WaitOnPrepared(translator);
        var sink = new MemorySink<string>(translator);
        pipeline.Start();

        Assert.That(sink.State, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(sink.State.Count(x => x == "foo"), Is.EqualTo(1));
            Assert.That(sink.State.Count(x => x == "bar"), Is.EqualTo(2));
            Assert.That(sink.State.Count(x => x == "quark"), Is.EqualTo(1));
        });
    }
}
