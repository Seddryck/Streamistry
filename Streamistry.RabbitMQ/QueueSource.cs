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

public class QueueSource : Source<string>
{
    private RabbitMQConnector Connector { get; }
    private string QueueName { get; }

    public QueueSource(RabbitMQConnector connector, string queueName, ObservabilityProvider? provider)
        : base(provider)
        => (Connector, QueueName) = (connector, queueName);

    protected QueueSource(Pipeline pipeline, RabbitMQConnector connector, string queueName)
        : base(pipeline)
        => (Connector, QueueName) = (connector, queueName);

    protected override bool TryReadNext(out string? item)
    {
        var result = Connector.GetChannel().BasicGet(QueueName, autoAck: true);

        if (result is not null)
        {
            var messageBody = result.Body.ToArray();
            item = Encoding.UTF8.GetString(messageBody);
            return true;
        }
        item = null;
        return false;
    }
}
