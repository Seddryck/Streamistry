using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;

namespace Streamistry.RabbitMQ.Testing;

[Category("RabbitMQ")]
public abstract class RabbitMQTests
{
    protected ConnectionFactory ConnectionFactory { get; set; }
    protected AdminRabbitMQ AdminRabbitMQ { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        ConnectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
        };
        AdminRabbitMQ = new(ConnectionFactory);

        try
        {
            AdminRabbitMQ.Channel.QueueDeclarePassive("streamistry-Q");
        }
        catch (OperationInterruptedException)
        {
            AdminRabbitMQ.Reset();
            AdminRabbitMQ.Channel.QueueDeclare(
                    queue: "streamistry-Q",
                    durable: false,
                    exclusive: false
                );
        }

        try
        {
            AdminRabbitMQ.Channel.ExchangeDeclarePassive("streamistry-X");
        }
        catch (OperationInterruptedException)
        {
            AdminRabbitMQ.Reset();
            AdminRabbitMQ.Channel.ExchangeDeclare(
                    exchange: "streamistry-X",
                    type: "direct"
                );
        }

        AdminRabbitMQ.Channel.QueueBind(
                queue: "streamistry-Q",
                exchange: "streamistry-X",
                routingKey: string.Empty
            );

        TearDown();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TearDown();
        AdminRabbitMQ.Channel.Dispose();
    }

    [TearDown]
    public void TearDown()
    {
        AdminRabbitMQ.Channel.QueuePurge("streamistry-Q");
    }

    public uint CountMessagesOnQueue()
    {
        var queueDeclareOk = AdminRabbitMQ.Channel.QueueDeclarePassive("streamistry-Q");
        var messageCount = queueDeclareOk.MessageCount;
        return messageCount;
    }
}
