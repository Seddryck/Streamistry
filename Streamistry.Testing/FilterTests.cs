using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class FilterTests
{
    [Test]
    public void Emit_InlinePositive_Successful()
    {
        var filter = new Filter<int>(x => x >= 0);
        Assert.Multiple(() =>
        {
            Assert.That(filter.EmitAndAnyOutput(7), Is.True);
            Assert.That(filter.EmitAndSingleOutput(8), Is.True);
            Assert.That(filter.EmitAndGetOutput(11), Is.EqualTo(11));
        });
    }

    [Test]
    public void Emit_InlinePositive_DontEmit()
    {
        var filter = new Filter<int>(x => x >= 0);
        Assert.That(filter.EmitAndAnyOutput(-10), Is.False);
    }
}
