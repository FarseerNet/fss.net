using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientRegister: ISingletonDependency
    {
        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        void UpdateClient(ClientVO client);

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        Task<List<ClientVO>> ToListAsync();

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        bool IsExists(long clientId);

        /// <summary>
        /// 移除客户端
        /// </summary>
        Task RemoveAsync(long clientId);

        /// <summary>
        /// 取出全局客户端
        /// </summary>
        Task<ClientVO> ToInfoAsync(long clientId);
    }
}