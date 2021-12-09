using System;
using System.Threading.Tasks;
using FS.DI;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientRegister: ISingletonDependency
    {

        /// <summary>
        /// 移除客户端
        /// </summary>
        Task RemoveAsync(long clientId);

    }
}