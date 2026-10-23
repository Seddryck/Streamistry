using System.Globalization;
using NUnit.Framework;
using Streamistry.Fluent;
using Streamistry.Testability;

namespace Streamistry.Expressif.Testing;

public class ScalarAdapterTests
{
    [Test]
    public void ConstantUsesExpressionVariableAndPreservesNull()
    {
        var constant = new PipelineBuilder().Source(new[] { 1, 2 }).UsingExpressif().Constant<string?>(null);
        var port = constant.BuildPipeElement();
        Assert.That(port.GetOutputs(constant.Build().Start), Is.EqualTo(new string?[] { null, null }));
    }

    [Test]
    public void CheckedConversionsPreserveTypesAndRejectLossOrOverflow()
    {
        Assert.That(ScalarAdapters.Convert<object, int>()(2m), Is.EqualTo(2));
        Assert.That(ScalarAdapters.Convert<object, decimal>()("2.5"), Is.EqualTo(2.5m));
        Assert.Throws<InvalidCastException>(() => ScalarAdapters.Convert<object, int>()(2.5m));
        Assert.Throws<InvalidCastException>(() => ScalarAdapters.Convert<object, int>()(long.MaxValue));
        Assert.Throws<InvalidCastException>(() => ScalarAdapters.Convert<object, decimal>()("bad"));
    }

    [Test]
    public void NullIsAcceptedOnlyByNullableOutputs()
    {
        Assert.That(ScalarAdapters.Convert<object?, int?>()(null), Is.Null);
        Assert.That(ScalarAdapters.Convert<object, string?>()(DBNull.Value), Is.Null);
        Assert.Throws<InvalidCastException>(() => ScalarAdapters.Convert<object?, int>()(null));
    }

    [Test]
    public void InvariantDatesAndExplicitClrCultureRemainAvailable()
    {
        Assert.That(ScalarAdapters.Convert<string, DateOnly?>()("2025-12-31"), Is.EqualTo(new DateOnly(2025, 12, 31)));
        Assert.Throws<InvalidCastException>(() => ScalarAdapters.Convert<string, DateOnly?>()("bad"));
        Assert.That(DateOnly.Parse("31/12/2025", CultureInfo.GetCultureInfo("fr-FR")), Is.EqualTo(new DateOnly(2025, 12, 31)));
        Assert.Throws<NotSupportedException>(() => ScalarAdapters.Convert<object, Uri>());
    }
}
