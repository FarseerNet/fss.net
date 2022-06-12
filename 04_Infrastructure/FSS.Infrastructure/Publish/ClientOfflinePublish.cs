using FS.Core.Abstract.EventBus;
using FS.DI;
using FSS.Domain.Client.Clients.Publish;

namespace FSS.Infrastructure.Publish;

public class ClientOfflinePublish : IClientOfflinePublish
{
    public string EventName { get; set; } = "ClientOffline";

    /// <summary>
    ///     发布客户端离线事件
    /// </summary>
    public void Publish(object sender, object message)
    {
        IocManager.GetService<IEventProduct>(name: EventName).SendSync(sender: sender, message: message);
    }
}