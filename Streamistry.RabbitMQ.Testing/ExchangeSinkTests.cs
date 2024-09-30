using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Streamistry.RabbitMQ.Testing;

public class ExchangeSinkTests : RabbitMQTests
{
    [Test]
    public void Emit_SingleMessage_MessageOnCorrespondingQueue()
    {
        using var connector = new RabbitMQConnector(ConnectionFactory);
        var sink = new ExchangeSink(connector, "streamistry-X");

        Warn.If(CountMessagesOnQueue(), Is.GreaterThan(0), "Queue is not empty.");

        sink.Emit("foo");
        Thread.Sleep(25); // transport from exchange to queue is handled by RabbitMQ and is async
        Assert.That(CountMessagesOnQueue(), Is.EqualTo(1));
    }
}
