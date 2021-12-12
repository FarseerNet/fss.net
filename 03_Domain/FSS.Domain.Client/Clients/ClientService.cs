using System;
using System.Threading.Tasks;
using FS.DI;
using FSS.Domain.Client.Clients.Interface;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Domain.Client.Clients
{
    public class ClientService : IClientService
    {
        public IClientRepository ClientRepository { get; set; }

        /// <summary>
        /// 检查超时离线的客户端
        /// </summary>
        public async Task CheckTimeoutAsync()
        {
            var lst = await ClientRepository.ToListAsync();
            foreach (var client in lst)
            {
                // 心跳大于1分钟，认为已经下线了
                if ((DateTime.Now - client.ActivateAt).TotalMinutes >= 1)
                {
                    await client.RemoveAsync();
                    IocManager.GetService<ClientPublish>().ClientOffline(this, client);
                }
            }
        }

        /// <summary>
        /// 获取客户端
        /// </summary>
        public Task<Entity.Client> ToEntity(long clientId) => ClientRepository.ToEntityAsync(clientId);
    }
}