using FS.DI;
using FS.EventBus;

namespace FSS.Domain.Client.Client
{
    public class ClientPublish : ISingletonDependency
    {
        /// <summary>
        /// 发布客户端离线事件
        /// </summary>
        public void ClientOffline(object sender, long clientId)
        {
            IocManager.GetService<IEventProduct>("ClientOffline").SendSync(sender, clientId);
        }
    }
}