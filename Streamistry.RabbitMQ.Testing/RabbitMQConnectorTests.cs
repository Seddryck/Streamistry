using NUnit.Framework;
using RabbitMQ.Client;

namespace Streamistry.RabbitMQ.Testing;

public class RabbitMQConnectorTests : RabbitMQTests
{
    [Test]
    public void GetChannel_OnSameThread_ReusesOpenChannel()
    {
        using var connector = new RabbitMQConnector(ConnectionFactory);

        var original = connector.GetChannel();
        Assert.That(connector.GetChannel(), Is.SameAs(original));
    }

    [Test]
    public async Task GetChannel_AfterChannelClosed_CreatesUsableChannel()
    {
        using var connector = new RabbitMQConnector(ConnectionFactory);
        var original = connector.GetChannel();
        // Keep channel lookup on this thread, since channels belong to pipeline threads.
        original.CloseAsync().GetAwaiter().GetResult();

        var replacement = connector.GetChannel();
        Assert.That(replacement, Is.Not.SameAs(original));
        Assert.That(replacement.IsOpen, Is.True);
        await replacement.QueueDeclarePassiveAsync("streamistry-Q");
    }

    [Test]
    public void Dispose_Repeatedly_ClosesChannelsAndRejectsFurtherUse()
    {
        var connector = new RabbitMQConnector(ConnectionFactory);
        var channel = connector.GetChannel();

        connector.Dispose();
        connector.Dispose();

        Assert.That(channel.IsOpen, Is.False);
        Assert.That(() => connector.GetChannel(), Throws.TypeOf<ObjectDisposedException>());
    }
}
