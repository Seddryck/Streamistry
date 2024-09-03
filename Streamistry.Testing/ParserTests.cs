using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Streamistry.Pipes.Parsers;
using Streamistry.Testability;

namespace Streamistry.Testing;
public class ParserTests
{
    [Test]
    public void DateParserEmit_ValidData_MainRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateParser(pipeline);
        Assert.Multiple(() =>
        {
            Assert.That(parser.EmitAndGetOutput("2024-08-30"), Is.EqualTo(new DateOnly(2024, 8, 30)));
            Assert.That(parser.EmitAndGetOutput("2024-08-31"), Is.EqualTo(new DateOnly(2024, 8, 31)));
            Assert.That(parser.EmitAndGetOutput("2024-09-01"), Is.EqualTo(new DateOnly(2024, 9, 1)));
        });
    }

    [Test]
    public void DateParserEmit_MixedData_MainRouteAndExceptionRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateParser(pipeline);
        Assert.Multiple(() =>
        {
            Assert.That(parser.EmitAndGetOutput("2024-08-30"), Is.EqualTo(new DateOnly(2024, 8, 30)));
            Assert.That(parser.EmitAndAnyAlternateOutput("2024-08-30"), Is.False);
            Assert.That(parser.EmitAndGetOutput("2024-08-31"), Is.EqualTo(new DateOnly(2024, 8, 31)));
            Assert.That(parser.EmitAndAnyAlternateOutput("2024-08-31"), Is.False);
            Assert.That(parser.EmitAndAnyOutput("2024-08-32"), Is.False);
            Assert.That(parser.EmitAndGetAlternateOutput("2024-08-32"), Is.EqualTo("2024-08-32"));
        });
    }

    [Test]
    public void DateTimeParserEmit_ValidData_MainRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateTimeParser(pipeline);
        Assert.Multiple(() =>
        {
            Assert.That(parser.EmitAndGetOutput("2024-08-30 17:12:16"), Is.EqualTo(new DateTime(2024, 8, 30, 17, 12, 16)));
            Assert.That(parser.EmitAndGetOutput("2024-08-31T17:12:16"), Is.EqualTo(new DateTime(2024, 8, 31, 17, 12, 16)));
            Assert.That(parser.EmitAndGetOutput("2024-09-01 05:00AM"), Is.EqualTo(new DateTime(2024, 9, 1, 5, 0, 0)));
        });
    }

    [Test]
    public void DateTimeParserEmit_MixedData_MainRouteAndExceptionRoute()
    {
        var pipeline = new Pipeline<string>();
        var parser = new DateTimeParser(pipeline);
        Assert.Multiple(() =>
        {
            Assert.That(parser.EmitAndGetOutput("2024-08-30 17:12:16"), Is.EqualTo(new DateTime(2024, 8, 30, 17, 12, 16)));
            Assert.That(parser.EmitAndAnyOutput("2024-08-31 25:62:41"), Is.False);
            Assert.That(parser.EmitAndAnyAlternateOutput("2024-08-31 25:62:41"), Is.True);
            Assert.That(parser.EmitAndGetOutput("2024-09-01 05:00PM"), Is.EqualTo(new DateTime(2024, 9, 1, 17, 0, 0)));
        });
    }
}
