using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Streamistry.RabbitMQ;
public class RabbitMQConnector : IDisposable
{
    private ConnectionFactory ConnectionFactory { get; }
    private IConnection Connection { get; set; }
    private ConcurrentDictionary<int, IModel> Channels { get; } = new();
    private readonly object @lock = new();

    public RabbitMQConnector(string hostName, string userName, string password)
        : this (new ConnectionFactory() { HostName = hostName, UserName = userName, Password = password })
    { }

    public RabbitMQConnector(ConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
        Connection = CreateConnection();
    }

    private IConnection CreateConnection()
    {
        lock (@lock)
        {
            foreach (var channel in Channels.Values)
            {
                try
                {
                    channel.ModelShutdown -= OnChannelShutdown;
                    channel?.Close();
                    channel?.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while closing channel: {ex.Message}");
                }
            }
            Channels.Clear();

            if (Connection != null)
            {
                Connection.ConnectionShutdown -= OnConnectionShutdown;
                Connection.Dispose();
            }

            var connection = ConnectionFactory.CreateConnection();
            connection.ConnectionShutdown += OnConnectionShutdown;
            return connection;
        }
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        CreateConnection();
    }

    public IModel GetChannel()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var threadId = Environment.CurrentManagedThreadId;
        return Channels.GetOrAdd(threadId, _ =>
        {
            lock (@lock)
            {
                if (Connection is null || !Connection.IsOpen)
                    Connection = ConnectionFactory.CreateConnection();
            }

            var channel = Connection.CreateModel();
            channel.ModelShutdown += OnChannelShutdown;

            return channel;
        });
    }

    private void OnChannelShutdown(object? sender, ShutdownEventArgs e)
    {
        if (sender is not null && sender is IModel)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Channel for thread {threadId} closed. Reason: {e.ReplyText}");
            Channels.TryRemove(threadId, out _);
        }
    }

    #region IDisposable
    private bool disposed = false;
    public void Dispose()
    {
        if (disposed)
            return;

        foreach (var channel in Channels.Values)
        {
            if (channel != null)
            {
                channel.ModelShutdown -= OnChannelShutdown;
                channel.Close();
                channel.Dispose();
            }
        }
        Channels.Clear();

        if (Connection != null)
        {
            Connection.ConnectionShutdown -= OnConnectionShutdown;
            Connection?.Close();
            Connection?.Dispose();
        }

        disposed = true;

        GC.SuppressFinalize(this);
    }
    #endregion
}
