using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Mappers;
using Streamistry.Pipes.Sinks;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

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

        Assert.That(zipper.GetOutputs(pipeline.Start), Is.EqualTo(new int[] { 0, 1, 1, 2 }));
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

        var expected = new string[] { "foo", "bar", "foo", "foo", "bar", "foo" };
        Assert.That(translator.GetOutputs(pipeline.Start), Is.EqualTo(expected));
    }

    [Test]
    public void OnReady_Translator_SourceStartingAfterPreparation()
    {
        var dictSource = new EnumerableSource<KeyValuePair<int, string>>
                        (
                            [new KeyValuePair<int, string>(0, "foo")
                            , new KeyValuePair<int, string>(1, "bar")]
                        );
        var valueSource = new EnumerableSource<int>([0, 1, -1, 0, 1, 0]);

        var pipeline = new Pipeline(dictSource);
        var increment = new Mapper<int, int>(valueSource, x => x += 1);
        var translator = new Translator<int, string>(increment, dictSource, x => "quark");
        valueSource.WaitOnPrepared(translator);

        var expected = new string[] { "bar", "quark", "foo", "bar", "quark", "bar" };
        Assert.That(translator.GetOutputs(pipeline.Start), Is.EqualTo(expected));
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

        var expected = new string[] { "foo", "bar", "bar", "quark" };
        Assert.That(translator.GetOutputs(pipeline.Start), Is.EqualTo(expected));
    }
}
