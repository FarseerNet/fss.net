using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity;

namespace FSS.Application.Client.Interface
{
    public interface IClientApp: ISingletonDependency
    {
        /// <summary>
        /// 检查超时离线的客户端
        /// </summary>
        Task CheckTimeoutAsync();
        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        Task<List<ClientVO>> ToListAsync();
    }
}