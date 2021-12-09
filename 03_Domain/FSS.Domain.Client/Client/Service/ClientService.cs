using System;
using System.Threading.Tasks;
using FS.DI;
using FS.EventBus;
using FSS.Domain.Client.Client.Interface;
using FSS.Infrastructure.Repository.Client.Interface;

namespace FSS.Domain.Client.Client.Service
{
    public class ClientService : IClientService
    {
        public IClientAgent ClientAgent { get; set; }

        /// <summary>
        /// 检查超时离线的客户端
        /// </summary>
        public async Task CheckTimeoutAsync()
        {
            var lst = await ClientAgent.ToListAsync();
            foreach (var client in lst)
            {
                // 心跳大于1秒中，认为已经下线了
                if ((DateTime.Now - client.ActivateAt).TotalMinutes >= 1)
                {
                    IocManager.GetService<IEventProduct>("ClientOffline").Send(this, client.Id.ToString());
                }
            }
        }
    }
}