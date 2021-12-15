using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;

namespace FSS.Domain.Client.Clients.Repository
{
    public interface IClientRepository: ISingletonDependency
    {
        /// <summary>
        /// 获取客户端列表
        /// </summary>
        Task<List<Entity.Client>> ToListAsync();

        /// <summary>
        /// 移除客户端
        /// </summary>
        Task RemoveClientAsync(Entity.Client client);
        
        /// <summary>
        /// 获取客户端
        /// </summary>
        Task<Entity.Client> ToEntityAsync(long clientId);
        
        /// <summary>
        /// 更新客户端的使用时间
        /// </summary>
        void Update(Entity.Client client);
        
        /// <summary>
        /// 客户端数量
        /// </summary>
        Task<long> GetCountAsync();
    }
}