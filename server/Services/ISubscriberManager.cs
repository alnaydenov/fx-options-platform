using System.Net.WebSockets;

namespace FxOptions.Server.Services;

public interface ISubscriberManager
{
    void Add(string id, WebSocket socket);
    void Remove(string id);
    IReadOnlyCollection<KeyValuePair<string, WebSocket>> GetAll();
    bool HasSubscribers { get; }
}
