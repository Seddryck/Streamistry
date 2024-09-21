using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Streamistry.Testing.Pipes.Sources;
public class DataReaderSourceTests
{
    private class DataReaderHelper
    {
        public static Mock<IDataReader> Create()
        {
            var dr = new Mock<IDataReader>();
            dr.SetupSequence(m => m.Read()).Returns(true).Returns(true).Returns(false);
            dr.Setup(m => m.FieldCount).Returns(2);
            dr.SetupSequence(m => m.GetName(0)).Returns("Id").Returns("Id");
            dr.SetupSequence(m => m.GetName(1)).Returns("Name").Returns("Name");
            dr.SetupSequence(m => m.GetValue(0)).Returns(1).Returns(2);
            dr.SetupSequence(m => m.GetValue(1)).Returns("Foo").Returns("Bar");
            dr.SetupSequence(m => m.GetInt32(0)).Returns(1).Returns(2);
            dr.SetupSequence(m => m.GetString(1)).Returns("Foo").Returns("Bar");
            return dr;
        }
    }

    [Test]
    public void Read_Array_Successful()
    {
        var dr = DataReaderHelper.Create();
        var source = new DataReaderAsArraySource(dr.Object);
        var pipeline = new Pipeline(source);
        Assert.That(source.GetOutputs(pipeline.Start), Is.EqualTo(new object[]
            { new object[] { 1, "Foo" }, new object[] { 2, "Bar" } }));
    }

    [Test]
    public void Read_Dictionary_Successful()
    {
        var dr = DataReaderHelper.Create();
        var source = new DataReaderAsDictionarySource(dr.Object);
        var pipeline = new Pipeline(source);
        var data = source.GetOutputs(pipeline.Start);
        Assert.That(data, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(data[0]!.ContainsKey("Id"), Is.True);
            Assert.That(data[0]!.ContainsKey("Name"), Is.True);
            Assert.That(data[0]!["Id"], Is.EqualTo(1));
            Assert.That(data[0]!["Name"], Is.EqualTo("Foo"));
            Assert.That(data[1]!.ContainsKey("Id"), Is.True);
            Assert.That(data[1]!.ContainsKey("Name"), Is.True);
            Assert.That(data[1]!["Id"], Is.EqualTo(2));
            Assert.That(data[1]!["Name"], Is.EqualTo("Bar"));
        });
    }

    [Test]
    public void Read_Value_Successful()
    {
        var dr = DataReaderHelper.Create();
        var source = new DataReaderAsValueSource<int>(dr.Object);
        var pipeline = new Pipeline(source);
        var data = source.GetOutputs(pipeline.Start);
        Assert.That(data, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(data, Does.Contain(1));
            Assert.That(data, Does.Contain(2));
        });
    }
}
