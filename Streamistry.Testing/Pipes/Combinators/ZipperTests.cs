using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Testability;
using Streamistry.Pipes.Combinators;

namespace Streamistry.Testing.Pipes.Combinators;
public class ZipperTests
{
    [Test]
    public void Emit_ZipWhenBothEmitted_Successful()
    {
        var first = new EmptySource<char>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<char, int, string>(first, second, (x, y) => new string(x, y));

        Assert.Multiple(() =>
        {
            Assert.That(first.EmitAndAnyOutputs('*', [first, second, combinator]), Is.EqualTo(new bool[] { true, false, false }));
            Assert.That(second.EmitAndAnyOutputs(5, [first, second, combinator]), Is.EqualTo(new bool[] { false, true, true }));
        });
    }

    [Test]
    public void Emit_ZipWhenBothEmitted_ZippedResult()
    {
        var first = new EmptySource<char>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<char, int, string>(first, second, (x, y) => new string(x, y));

        first.Emit('*');
        Assert.That(second.EmitAndGetOutput(5, combinator), Is.EqualTo("*****"));
    }

    [Test]
    public void Emit_ZipSameType_Successful()
    {
        var first = new EmptySource<int>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<int, int, int>(first, second, (x, y) => x * y);

        Assert.Multiple(() =>
        {
            Assert.That(second.EmitAndAnyOutputs(5, [first, second, combinator]), Is.EqualTo(new bool[] { false, true, false }));
            Assert.That(second.EmitAndAnyOutputs(8, [first, second, combinator]), Is.EqualTo(new bool[] { false, true, false }));
            Assert.That(first.EmitAndAnyOutputs(10, [first, second, combinator]), Is.EqualTo(new bool[] { true, false, true }));
        });
    }

    [Test]
    public void Emit_ZipSameType_ExpectedResult()
    {
        var first = new EmptySource<int>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<int, int, int>(first, second, (x, y) => x * y);

        second.Emit(5);
        second.Emit(8);
        Assert.That(first.EmitAndGetOutput(10, combinator), Is.EqualTo(50));
    }

    [Test]
    public void Emit_ZipWhenBothEmittedReversed_Successful()
    {
        var first = new EmptySource<char>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<char, int, string>(first, second, (x, y) => new string(x, y));

        Assert.Multiple(() =>
        {
            Assert.That(second.EmitAndAnyOutputs(5, [first, second, combinator]), Is.EqualTo(new bool[] { false, true, false }));
            Assert.That(first.EmitAndGetOutput('*', combinator), Is.EqualTo("*****"));
        });
    }

    [Test]
    public void Emit_ZipWhenBothEmittedNotInSync_Successful()
    {
        var first = new EmptySource<char>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<char, int, string>(first, second, (x, y) => new string(x, y));

        Assert.Multiple(() =>
        {
            Assert.That(first.EmitAndAnyOutput('*', combinator), Is.False);
            Assert.That(first.EmitAndAnyOutput('!', combinator), Is.False);
            Assert.That(second.EmitAndGetOutput(5, combinator), Is.EqualTo("*****"));
            Assert.That(second.EmitAndGetOutput(3, combinator), Is.EqualTo("!!!"));
        });
    }

    [Test]
    public void Emit_ZipThreeStreams_Successful()
    {
        var year = new EmptySource<int>();
        var month = new EmptySource<int>();
        var day = new EmptySource<int>();
        var combinator = new Zipper<int, int, int, DateOnly>(year, month, day, (x, y, z) => new DateOnly(x, y, z));

        year.Emit(2024);
        month.Emit(9);
        Assert.That(day.EmitAndGetOutput(15, combinator), Is.EqualTo(new DateOnly(2024, 9, 15)));
    }

    [Test]
    public void Emit_ZipSixStreams_Successful()
    {
        var year = new EmptySource<int>();
        var month = new EmptySource<int>();
        var day = new EmptySource<int>();
        var hour = new EmptySource<int>();
        var minute = new EmptySource<int>();
        var second = new EmptySource<int>();
        var combinator = new Zipper<int, int, int, int, int, int, DateTime>(year, month, day, hour, minute, second, (y, m, d, h, i, s) => new DateTime(y, m, d, h, i, s));

        year.Emit(2024);
        month.Emit(9);
        hour.Emit(17);
        second.Emit(45);
        minute.Emit(12);
        Assert.That(day.EmitAndGetOutput(15, combinator), Is.EqualTo(new DateTime(2024, 9, 15, 17, 12, 45)));
    }
}
