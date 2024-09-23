using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Mappers;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Mappers;
public class ConstantTests
{
    [Test]
    public void Emit_ConstantInt_Success()
    {
        var caster = new Constant<string?, int>(1);
        var output = caster.EmitAndGetOutput("foo");

        Assert.That(output, Is.EqualTo(1));
    }
}
