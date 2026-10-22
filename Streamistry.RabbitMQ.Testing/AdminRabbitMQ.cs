using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Streamistry.RabbitMQ.Testing;
public class AdminRabbitMQ : IDisposable
{
    public ConnectionFactory ConnectionFactory { get; private set; }
    public IConnection Connection { get; private set; }
    public IChannel Channel { get; private set; }

    public AdminRabbitMQ(ConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
        Connection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
        Channel = Connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        Channel?.Dispose();
        Connection?.Dispose();
    }

    internal void Reset()
    {
        Channel?.Dispose();
        Connection.Dispose();
        Connection = ConnectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
        Channel = Connection.CreateChannelAsync().GetAwaiter().GetResult();
    }
}
