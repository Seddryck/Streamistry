using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Streamistry.Observability;

namespace Streamistry.RabbitMQ;

public class ExchangeSink : Sink<string>
{
    public ExchangeSink(RabbitMQConnector connector, string exchangeName)
        : base(x => Publish(connector, exchangeName, x!))
    { }

    public ExchangeSink(IChainablePort<string> upstream, RabbitMQConnector connector, string exchangeName)
        : base(upstream, x => Publish(connector, exchangeName, x!))
    { }

    protected static void Publish(RabbitMQConnector connector, string exchangeName, string item)
    {
        var body = Encoding.UTF8.GetBytes(item);
        connector.GetChannel()
            .BasicPublish(exchange: exchangeName, routingKey:string.Empty, body: body);
    }
}

public class ExchangeSink<TInput> : Sink<TInput>
{
    protected ExchangeSink(IChainablePort<TInput> upstream, RabbitMQConnector connector
        , Func<TInput, string> exchangeName
        , Func<TInput, string> routingKey
        , Func<TInput, string> body)
        : base(upstream, x => Publish(connector, exchangeName.Invoke(x!), routingKey.Invoke(x!), body.Invoke(x!)))
    { }

    protected static void Publish(RabbitMQConnector connector, string exchangeName, string routingKey, string item)
    {
        var body = Encoding.UTF8.GetBytes(item);
        connector.GetChannel()
            .BasicPublish(exchange: exchangeName, routingKey: routingKey, body: body);
    }
}
