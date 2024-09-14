using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Aggregators;
public class CountTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var aggregator = new Count<int>();
        Assert.That(aggregator.EmitAndGetOutput(10), Is.EqualTo(1));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var aggregator = new Count<int>();
        Assert.That(aggregator.EmitAndAnyOutput(2), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(3), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(4), Is.True);
    }

    [Test]
    public void Emit_ManyElements_CorrectResults()
    {
        var aggregator = new Count<int>();
        Assert.That(aggregator.EmitAndGetOutput(2), Is.EqualTo(1));
        Assert.That(aggregator.EmitAndGetOutput(3), Is.EqualTo(2));
        Assert.That(aggregator.EmitAndGetOutput(4), Is.EqualTo(3));
    }

    [Test]
    public void Emit_ManyElementsAsShort_Successful()
    {
        var aggregator = new Count<int, short>();
        aggregator.Emit(10);
        aggregator.Emit(15);
        Assert.That(aggregator.EmitAndGetOutput(4), Is.EqualTo((short)3));
    }
}
