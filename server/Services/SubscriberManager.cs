using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace FxOptions.Server.Services;

public class SubscriberManager : ISubscriberManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _subscribers = new();

    public void Add(string id, WebSocket socket)
    {
        _subscribers.TryAdd(id, socket);
    }

    public void Remove(string id)
    {
        _subscribers.TryRemove(id, out _);
    }

    public IReadOnlyCollection<KeyValuePair<string, WebSocket>> GetAll()
    {
        return _subscribers.ToArray();
    }

    public bool HasSubscribers => !_subscribers.IsEmpty;
}
