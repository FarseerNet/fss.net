using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Application.Clients.Client.Entity;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Application.Clients.Client
{
    public class ClientApp : ISingletonDependency
    {
        public IClientRepository ClientRepository { get; set; }

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        public Task<List<ClientDTO>> ToListAsync() => ClientRepository.ToListAsync().MapAsync<ClientDTO, Domain.Client.Clients.Entity.Client>();

        /// <summary>
        /// 更新客户端的使用时间
        /// </summary>
        public void UpdateClient(Domain.Client.Clients.Entity.Client client) => client.Update();
        
        /// <summary>
        /// 客户端数量
        /// </summary>
        public Task<long> GetCountAsync() => ClientRepository.GetCountAsync();
    }
}