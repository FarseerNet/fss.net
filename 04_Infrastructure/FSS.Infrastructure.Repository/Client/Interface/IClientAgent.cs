using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Infrastructure.Repository.Client.Model;

namespace FSS.Infrastructure.Repository.Client.Interface
{
    public interface IClientAgent : ISingletonDependency
    {
        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        void UpdateClient(ClientPO client);
        /// <summary>
        /// 取出全局客户端
        /// </summary>
        Task<ClientPO> ToInfoAsync(long clientId);
        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        Task<List<ClientPO>> ToListAsync();
        /// <summary>
        /// 移除客户端
        /// </summary>
        Task RemoveClientAsync(long id);
        /// <summary>
        /// 取出全局客户端数量（fops在用）
        /// </summary>
        Task<long> GetClientCountAsync();
    }
}