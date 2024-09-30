using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RabbitMQ.Client;

namespace Streamistry.RabbitMQ.Testing;

public class QueueSourceTests : RabbitMQTests
{
    [SetUp]
    public void Setup()
    {
        foreach (var item in new string[] { "foo", "bar" })
            AdminRabbitMQ.Channel.BasicPublish(exchange: "streamistry-X", routingKey: string.Empty, body: Encoding.UTF8.GetBytes(item));
    }

    [Test]
    public void Start_WithMessages_AllReadAndQueueEmpty()
    {
        int readCount = 0;
        using var connector = new RabbitMQConnector(ConnectionFactory);
        var source = new QueueSource(connector, "streamistry-Q", null);
        source.RegisterDownstream((x) => readCount += 1);

        var toReadCount = CountMessagesOnQueue();
        Warn.If(toReadCount, Is.EqualTo(0), "No messages to read");

        source.Start();
        Assert.Multiple(() =>
        {
            Assert.That(readCount, Is.EqualTo(toReadCount));
            Assert.That(CountMessagesOnQueue(), Is.EqualTo(0));
        });
    }
}
