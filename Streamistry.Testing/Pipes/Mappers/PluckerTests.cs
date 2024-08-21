using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Mappers;
using Streamistry.Pipes.Sinks;

namespace Streamistry.Testing.Pipes.Mappers;
public class PluckerTests
{
    private record class Human(string Name, DateOnly BirthDay)
    { }

    [Test]
    public void Emit_HumanPluckerOnBirthDay_BirthDay()
    {
        var pipeline = new Pipeline<Human>();
        var plucker = new Plucker<Human, DateOnly>(pipeline, h => h.BirthDay);
        var sink = new MemorySink<DateOnly>(plucker);

        pipeline.Emit(new Human("Albert Einstein", new DateOnly(1879, 3, 14)));
        Assert.That(sink.State.Last(), Is.EqualTo(new DateOnly(1879, 3, 14)));
    }

    [Test]
    public void Emit_HumanPluckerOnBirthMonth_Integer()
    {
        var pipeline = new Pipeline<Human>();
        var plucker = new Plucker<Human, int>(pipeline, h => h.BirthDay.Month);
        var sink = new MemorySink<int>(plucker);

        pipeline.Emit(new Human("Albert Einstein", new DateOnly(1879, 3, 14)));
        Assert.That(sink.State.Last(), Is.EqualTo(3));
    }
}
