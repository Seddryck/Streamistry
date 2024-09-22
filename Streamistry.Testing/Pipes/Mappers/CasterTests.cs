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
public class CasterTests
{
    private record class Human(string Name, DateOnly BirthDay);
    private record class Scientist(string Name, DateOnly BirthDay, string Field) : Human(Name, BirthDay);
    private record class Poet(string Name, DateOnly BirthDay, string Trauma) : Human(Name, BirthDay);

    [Test]
    public void Emit_CasterIntToLongWithPrimitive_Success()
    {
        var caster = new Caster<int, long>();
        var output = caster.EmitAndGetOutput(10);

        Assert.That(output, Is.EqualTo(10L));
    }

    [Test]
    public void Emit_CasterIntToDecimalWithImplicitOperator_Success()
    {
        var caster = new Caster<int, decimal>();
        var output = caster.EmitAndGetOutput(10);

        Assert.That(output, Is.TypeOf<decimal>());
        Assert.That(output, Is.EqualTo(new decimal(10)));
    }

    [Test]
    public void Emit_CasterScientistToHumanWithInheritance_Success()
    {
        var caster = new Caster<Scientist, Human>();
        var albert = new Scientist("Albert Einstein", new DateOnly(1879, 3, 14), "Physicist");
        var output = caster.EmitAndGetOutput(albert);

        Assert.That(output, Is.Not.Null);
        Assert.That(output, Is.AssignableTo<Human>());
        Assert.That(output, Is.EqualTo(albert));
    }

    [Test]
    public void Emit_CasterScientistToPoet_Failure()
    {
        var caster = new Caster<Scientist, Poet>();
        var albert = new Scientist("Albert Einstein", new DateOnly(1879, 3, 14), "Physicist");
        var ex = Assert.Throws<InvalidCastException>(() => caster.EmitAndGetOutput(albert));
        Assert.That(ex.Message, Does.Contain("Specified cast"));
        Assert.That(ex.Message, Does.Contain("is not valid."));
        Assert.That(ex.Message, Does.Contain("Scientist"));
        Assert.That(ex.Message, Does.Contain("Poet"));
    }

    [Test]
    public void Emit_CasterLongToInt_Failure()
    {
        var caster = new Caster<long, int>();
        var ex = Assert.Throws<InvalidCastException>(() => caster.EmitAndGetOutput(long.MaxValue));
        Assert.That(ex.Message, Does.Contain("Specified cast"));
        Assert.That(ex.Message, Does.Contain("is not valid."));
        Assert.That(ex.Message, Does.Contain("Int64"));
        Assert.That(ex.Message, Does.Contain("Int32"));
    }

    [Test]
    public void Emit_SafeCasterIntToLongWithPrimitive_Success()
    {
        var caster = new SafeCaster<int, long>();
        var output = caster.EmitAndGetOutput(10);

        Assert.That(output, Is.EqualTo(10L));
    }

    [Test]
    public void Emit_SafeCasterIntToDecimalWithImplicitOperator_Success()
    {
        var caster = new SafeCaster<int, decimal>();
        var output = caster.EmitAndGetOutput(10);

        Assert.That(output, Is.TypeOf<decimal>());
        Assert.That(output, Is.EqualTo(new decimal(10)));
    }

    [Test]
    public void Emit_SafeCasterScientistToHumanWithInheritance_Success()
    {
        var caster = new SafeCaster<Scientist, Human>();
        var albert = new Scientist("Albert Einstein", new DateOnly(1879, 3, 14), "Physicist");
        var output = caster.EmitAndGetOutput(albert);

        Assert.That(output, Is.Not.Null);
        Assert.That(output, Is.AssignableTo<Human>());
        Assert.That(output, Is.EqualTo(albert));
    }

    [Test]
    public void Emit_SafeCasterScientistToPoet_Failure()
    {
        var caster = new SafeCaster<Scientist, Poet>();
        var albert = new Scientist("Albert Einstein", new DateOnly(1879, 3, 14), "Physicist");
        Assert.That(caster.EmitAndAnyOutput(albert), Is.False);
    }

    [Test]
    public void Emit_SafeCasterLongToInt_Failure()
    {
        var caster = new SafeCaster<long, int>();
        Assert.That(caster.EmitAndAnyOutput(long.MaxValue), Is.False);
    }
}
