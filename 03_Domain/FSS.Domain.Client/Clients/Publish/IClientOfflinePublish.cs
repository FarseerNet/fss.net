using FS.DI;

namespace FSS.Domain.Client.Clients.Publish;

public interface IClientOfflinePublish : ISingletonDependency
{
    public string EventName { get; set; }

    /// <summary>
    ///     发布客户端离线事件
    /// </summary>
    void Publish(object sender, object message);
}