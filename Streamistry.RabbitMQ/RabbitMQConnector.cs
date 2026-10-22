using RabbitMQ.Client;

namespace Streamistry.RabbitMQ;

public class RabbitMQConnector : IDisposable
{
    private ConnectionFactory ConnectionFactory { get; }
    private IConnection Connection { get; set; }
    private Dictionary<int, IChannel> Channels { get; } = new();
    private readonly object sync = new();
    private bool disposed;

    public RabbitMQConnector(string hostName, string userName, string password)
        : this(new ConnectionFactory { HostName = hostName, UserName = userName, Password = password })
    { }

    public RabbitMQConnector(ConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
        Connection = ConnectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    public IChannel GetChannel()
    {
        // Bridge the synchronous pipeline API to the asynchronous RabbitMQ client.
        // Never reconnect from a shutdown callback: closing the connection there
        // can deadlock the client's event dispatcher.
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (!Connection.IsOpen)
            {
                foreach (var cached in Channels.Values)
                    cached.Dispose();
                Channels.Clear();
                Connection.Dispose();
                Connection = ConnectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            }

            var threadId = Environment.CurrentManagedThreadId;
            if (!Channels.TryGetValue(threadId, out var channel) || !channel.IsOpen)
            {
                channel?.Dispose();
                channel = Connection.CreateChannelAsync().GetAwaiter().GetResult();
                Channels[threadId] = channel;
            }

            return channel;
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed)
                return;
            disposed = true;

            try
            {
                foreach (var channel in Channels.Values)
                    channel.Dispose();
            }
            finally
            {
                Channels.Clear();
                Connection.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }
}
