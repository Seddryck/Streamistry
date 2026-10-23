using Expressif;
using NUnit.Framework;
using Streamistry.Fluent;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Expressif.Testing;

public class StructureAdapterTests
{
    [Test]
    public void NestedClrPropertiesAndMissingFieldsUseExpressif()
    {
        var row = StructuredValues.FromClr(new { customer = new { name = "Alice" } });
        Assert.That(Expression.Create(".customer.name").Evaluate(row), Is.EqualTo("Alice"));
        Assert.That(Expression.Create(".missing").Evaluate(row), Is.Null);
    }

    [Test]
    public void SplitCardinalityIsExplicit()
    {
        Assert.That(StructureAdapters.SplitResult<object?>(null), Is.EqualTo(new object?[] { null }));
        Assert.That(StructureAdapters.SplitResult<string>("abc"), Is.EqualTo(new[] { "abc" }));
        Assert.That(StructureAdapters.SplitResult<int>(Array.Empty<int>()), Is.Empty);
        Assert.That(StructureAdapters.SplitResult<int>(new[] { 1, 2 }), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void SplitAndPluckFluentAdaptersRunExpressions()
    {
        var builder = new PipelineBuilder().Source(new[] { new { names = new[] { "a", "b" } } })
            .UsingExpressif().Split<string>(".names");
        var port = builder.BuildPipeElement();
        Assert.That(port.GetOutputs(builder.Build().Start), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void LookupPreparationLastWinsAndFallbackPreservesMatchedNull()
    {
        var keys = new EnumerableSource<string>(["a", "b", "missing"]);
        var rows = new EnumerableSource<KeyValuePair<string, object?>>([
            new("a", "old"), new("a", "new"), new("b", null)]);
        var lookup = new ExpressionLookup<string?>(keys, rows, "\"fallback\"");
        Assert.Throws<InvalidOperationException>(() => lookup.Emit("a"));
        Assert.That(lookup.GetOutputs(new Pipeline(rows).Start), Is.EqualTo(new string?[] { "new", null, "fallback" }));
    }

    [Test]
    public void CombineAfterPairingUsesExpression()
    {
        var a = new EnumerableSource<string?>(["a", "b"]);
        var b = new EnumerableSource<string?>(["1", "2"]);
        var zip = StructureAdapters.ZipExpression<string, string, string>(a, b, ".item1 | append(^.item2)");
        Assert.That(zip.GetOutputs(new Pipeline([a, b]).Start), Is.EqualTo(new[] { "a1", "b2" }));
    }
}

