using System.Net.WebSockets;
using FxOptions.Server.Services;
using NSubstitute;

namespace FxOptions.Server.Tests;

public class SubscriberManagerTests
{
    private static WebSocket CreateMockSocket()
    {
        return Substitute.For<WebSocket>();
    }

    [Fact]
    public void HasSubscribers_InitiallyFalse()
    {
        var manager = new SubscriberManager();

        Assert.False(manager.HasSubscribers);
    }

    [Fact]
    public void Add_MakesHasSubscribersTrue()
    {
        var manager = new SubscriberManager();
        var socket = CreateMockSocket();

        manager.Add("client-1", socket);

        Assert.True(manager.HasSubscribers);
    }

    [Fact]
    public void Remove_AfterLastSubscriber_MakesHasSubscribersFalse()
    {
        var manager = new SubscriberManager();
        var socket = CreateMockSocket();
        manager.Add("client-1", socket);

        manager.Remove("client-1");

        Assert.False(manager.HasSubscribers);
    }

    [Fact]
    public void GetAll_ReturnsAllSubscribers()
    {
        var manager = new SubscriberManager();
        var socket1 = CreateMockSocket();
        var socket2 = CreateMockSocket();
        manager.Add("client-1", socket1);
        manager.Add("client-2", socket2);

        var all = manager.GetAll();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Add_DuplicateId_DoesNotThrow()
    {
        var manager = new SubscriberManager();
        var socket = CreateMockSocket();

        manager.Add("client-1", socket);
        manager.Add("client-1", socket); // duplicate — should not throw

        Assert.Single(manager.GetAll());
    }

    [Fact]
    public void Remove_NonexistentId_DoesNotThrow()
    {
        var manager = new SubscriberManager();

        manager.Remove("does-not-exist"); // should not throw

        Assert.False(manager.HasSubscribers);
    }
}
