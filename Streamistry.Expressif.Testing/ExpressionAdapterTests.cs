using Expressif;
using Moq;
using NUnit.Framework;
using Streamistry.Expressif;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Expressif.Testing;

public class ExpressionAdapterTests
{
    [Test]
    public void TextMappingBindsAndMaps()
    {
        var source = new EnumerableSource<string>([" a ", "b"]);
        var map = source.MapExpression<string, string>("trim | upper");
        Assert.That(map.GetOutputs(source.Start), Is.EqualTo(new[] { "A", "B" }));
    }

    [Test]
    public void PredicatesFilter()
    {
        var source = new EnumerableSource<object?>([null, 1, null, 2]);
        var filter = source.FilterExpression("is-not-null");
        Assert.That(filter.GetOutputs(source.Start), Is.EqualTo(new object[] { 1, 2 }));
    }

    [Test]
    public void NullIsSuccessAndIncompatibleOutputIsFailure()
    {
        var expression = new Mock<IExpression>();
        expression.Setup(x => x.WithContext(It.IsAny<EvaluationContext>())).Returns(expression.Object);
        expression.Setup(x => x.Evaluate(It.IsAny<object>())).Returns((object?)null);
        var source = new EnumerableSource<string>(["x"]);
        var map = new SafeMapper<string, string?>(source, ExpressionAdapters.Map<string, string?>(expression.Object));
        Assert.That(map.GetOutputs(source.Start), Is.EqualTo(new string?[] { null }));
        Assert.Throws<InvalidCastException>(() => ExpressionAdapters.Adapt<int>(null));
        Assert.Throws<InvalidCastException>(() => ExpressionAdapters.Adapt<int>(2m));
    }

    [Test]
    public void SafeMappingRoutesEvaluationExceptionsAndBadOutputToAlternate()
    {
        var source = new EnumerableSource<string>(["1", "2"]);
        var map = new SafeMapper<string, int>(source, ExpressionAdapters.Map<string, int>(Expression.Create("upper")));
        var rejected = new List<string>(); map.Alternate.RegisterDownstream(rejected.Add);
        Assert.That(map.GetOutputs(source.Start), Is.Empty);
        Assert.That(rejected, Is.EqualTo(new[] { "1", "2" }));
        var failing = new SafeMapper<string, string>(source, _ => throw new InvalidOperationException());
        Assert.That(failing.GetOutputs(source.Start), Is.Empty);
    }

    [Test]
    public void SharedExpressionHasIsolatedContextPerAdapter()
    {
        var expression = Expression.Create("append(@suffix)");
        var a = ExpressionAdapters.Map<string, string>(expression, new EvaluationContext(new Dictionary<string, object?> { ["suffix"] = "a" }));
        var b = ExpressionAdapters.Map<string, string>(expression, new EvaluationContext(new Dictionary<string, object?> { ["suffix"] = "b" }));
        Assert.That(a("x"), Is.EqualTo("xa"));
        Assert.That(b("x"), Is.EqualTo("xb"));
        Assert.That(a("x"), Is.EqualTo("xa"));
    }
}
