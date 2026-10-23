using System.Data;
using Moq;
using NUnit.Framework;
using Streamistry.Pipes.Sources;
using Streamistry.Testability;

namespace Streamistry.Testing.Pipes.Sources;

public class ReaderOwnershipTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void NormalCompletionHonorsOwnership(bool owned)
    {
        var reader = new Mock<IDataReader>();
        var source = new DataReaderAsArraySource(() => owned ? ReaderLease.Own(reader.Object) : ReaderLease.Borrow(reader.Object));
        source.Start();
        source.Dispose();
        reader.Verify(x => x.Dispose(), owned ? Times.Once() : Times.Never());
    }

    [Test]
    public void ReadFailureDisposesReaderAndDependencies()
    {
        var reader = new Mock<IDataReader>();
        var command = new Mock<IDisposable>();
        reader.Setup(x => x.Read()).Throws<InvalidOperationException>();
        var source = new DataReaderAsArraySource(() => ReaderLease.Own(reader.Object, command.Object));
        Assert.Throws<InvalidOperationException>(source.Start);
        reader.Verify(x => x.Dispose(), Times.Once());
        command.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public void EarlyStopDisposesOnce()
    {
        var reader = new Mock<IDataReader>();
        reader.Setup(x => x.Read()).Returns(true);
        var source = new DataReaderAsArraySource(() => ReaderLease.Own(reader.Object));
        source.RegisterDownstream(_ => source.Stop());
        source.Start();
        reader.Verify(x => x.Read(), Times.Once());
        reader.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public void FactoryReopensAndRowsAreIndependentOrderedSnapshots()
    {
        using var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, DBNull.Value);
        table.Rows.Add(2, "second");
        var source = new DataReaderAsDictionarySource(() => ReaderLease.Own(table.CreateDataReader()));
        var first = source.GetOutputs(source.Start);
        var second = source.GetOutputs(source.Start);
        Assert.That(first[0]!.Keys, Is.EqualTo(new[] { "Id", "Name" }));
        Assert.That(first[0]!["Name"], Is.Null);
        Assert.That(first[0]!["Id"], Is.EqualTo(1));
        Assert.That(second, Has.Length.EqualTo(2));
    }

    [Test]
    public void DuplicateNamesFailAndDispose()
    {
        var reader = new Mock<IDataReader>();
        reader.Setup(x => x.Read()).Returns(true);
        reader.Setup(x => x.FieldCount).Returns(2);
        reader.Setup(x => x.GetName(It.IsAny<int>())).Returns("same");
        var source = new DataReaderAsDictionarySource(() => ReaderLease.Own(reader.Object));
        Assert.Throws<ArgumentException>(source.Start);
        reader.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public void NullArraysAndValuesFollowSameContract()
    {
        using var table = new DataTable();
        table.Columns.Add("Value", typeof(string));
        table.Rows.Add(DBNull.Value);
        var arrays = new DataReaderAsArraySource(() => ReaderLease.Own(table.CreateDataReader()));
        var values = new DataReaderAsValueSource<string>(() => ReaderLease.Own(table.CreateDataReader()));
        Assert.That(arrays.GetOutputs(arrays.Start)[0][0], Is.Null);
        Assert.That(values.GetOutputs(values.Start), Is.EqualTo(new string?[] { null }));
    }
}
