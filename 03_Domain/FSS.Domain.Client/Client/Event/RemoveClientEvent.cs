using System.Threading.Tasks;
using FS.EventBus;
using FS.EventBus.Attr;
using FS.Extends;
using FSS.Infrastructure.Repository.Client.Interface;

namespace FSS.Domain.Client.Client.Event
{
    /// <summary>
    /// 客户端下线后移除
    /// </summary>
    [Consumer(EventName = "ClientOffline")]
    public class RemoveClientEvent : IListenerMessage
    {
        public IClientAgent ClientAgent { get; set; }

        public async Task<bool> Consumer(string message, object sender, DomainEventArgs ea)
        {
            await ClientAgent.RemoveClientAsync(message.ConvertType(0L));
            return true;
        }
    }
}