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
            AdminRabbitMQ.Channel.QueueDeclarePassiveAsync("streamistry-Q").GetAwaiter().GetResult();
        }
        catch (OperationInterruptedException)
        {
            AdminRabbitMQ.Reset();
            AdminRabbitMQ.Channel.QueueDeclareAsync(
                    queue: "streamistry-Q",
                    durable: false,
                    exclusive: false, autoDelete: false
                ).GetAwaiter().GetResult();
        }

        try
        {
            AdminRabbitMQ.Channel.ExchangeDeclarePassiveAsync("streamistry-X").GetAwaiter().GetResult();
        }
        catch (OperationInterruptedException)
        {
            AdminRabbitMQ.Reset();
            AdminRabbitMQ.Channel.ExchangeDeclareAsync(
                    exchange: "streamistry-X",
                    type: "direct", durable: false, autoDelete: false
                ).GetAwaiter().GetResult();
        }

        AdminRabbitMQ.Channel.QueueBindAsync(
                queue: "streamistry-Q",
                exchange: "streamistry-X",
                routingKey: string.Empty
            ).GetAwaiter().GetResult();

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
        AdminRabbitMQ.Channel.QueuePurgeAsync("streamistry-Q").GetAwaiter().GetResult();
    }

    public uint CountMessagesOnQueue()
    {
        var queueDeclareOk = AdminRabbitMQ.Channel.QueueDeclarePassiveAsync("streamistry-Q").GetAwaiter().GetResult();
        var messageCount = queueDeclareOk.MessageCount;
        return messageCount;
    }
}
