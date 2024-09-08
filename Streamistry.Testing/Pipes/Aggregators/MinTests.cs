using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Aggregators;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Aggregators;
public class MinTests
{
    [Test]
    public void Emit_SingleElement_Successful()
    {
        var aggregator = new Min<int>();
        Assert.That(aggregator.EmitAndGetOutput(10), Is.EqualTo(10));
    }

    [Test]
    public void Emit_ManyElements_Successful()
    {
        var aggregator = new Min<int>();
        Assert.That(aggregator.EmitAndAnyOutput(15), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(22), Is.True);
        Assert.That(aggregator.EmitAndAnyOutput(10), Is.True);
    }

    [Test]
    public void Emit_ManyElements_CorrectResults()
    {
        var aggregator = new Min<int>();
        Assert.That(aggregator.EmitAndGetOutput(15), Is.EqualTo(15));
        Assert.That(aggregator.EmitAndGetOutput(22), Is.EqualTo(15));
        Assert.That(aggregator.EmitAndGetOutput(10), Is.EqualTo(10));
    }
}
