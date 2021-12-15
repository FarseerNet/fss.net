using FS.DI;
using FS.EventBus;
using FSS.Domain.Client.Clients.Publish;

namespace FSS.Infrastructure.Publish
{
    public class ClientOfflinePublish : IClientOfflinePublish
    {
        public string EventName { get; set; } = "ClientOffline";
        
        /// <summary>
        /// 发布客户端离线事件
        /// </summary>
        public void Publish(object sender, object message)
        {
            IocManager.GetService<IEventProduct>(EventName).SendSync(sender, message);
        }
    }
}