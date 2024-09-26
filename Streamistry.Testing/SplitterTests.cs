using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class SplitterTests
{
    private static readonly string[] EmptyArray = Array.Empty<string>();

    [Test]
    public void Emit_Splitter_Successful()
    {
        var splitter = new Splitter<string?, string?>(x => x?.Split(';') ?? EmptyArray);
        Assert.Multiple(() =>
        {
            Assert.That(splitter.EmitAndGetOutput("foo;bar;quark"), Is.EqualTo("quark"));
            Assert.That(splitter.EmitAndGetManyOutputs("foo;bar;quark"), Is.EqualTo(new string[] { "foo", "bar", "quark" }));
        });
    }

    [Test]
    public void Emit_NullSplitter_Successful()
    {
        var splitter = new Splitter<string?, string?>(x => x?.Split(';') ?? EmptyArray);
        Assert.Multiple(() =>
        {
            Assert.That(splitter.EmitAndGetOutput(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(splitter.EmitAndGetOutput(null), Is.EqualTo(null));
        });
    }
}
