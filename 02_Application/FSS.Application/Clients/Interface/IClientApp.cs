using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Application.Clients.Dto;
using FSS.Domain.Client.Clients.Entity;

namespace FSS.Application.Clients.Interface
{
    public interface IClientApp : ISingletonDependency
    {
        /// <summary>
        /// 检查超时离线的客户端
        /// </summary>
        Task CheckTimeoutAsync();
        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        Task<List<ClientDTO>> ToListAsync();
        /// <summary>
        /// 客户端信息
        /// </summary>
        Task<Client> ToEntityAsync(long taskClientId);
        /// <summary>
        /// 更新客户端的使用时间
        /// </summary>
        void UpdateClient(Client client);
        /// <summary>
        /// 客户端数量
        /// </summary>
        Task<long> GetCountAsync();
    }
}