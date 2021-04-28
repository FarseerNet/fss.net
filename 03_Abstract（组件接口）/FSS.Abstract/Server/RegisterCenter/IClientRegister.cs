using FS.DI;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientRegister: ITransientDependency
    {
        /// <summary>
        /// 注册
        /// </summary>
        void Register(ClientConnectVO client);

        /// <summary>
        /// 移除客户端
        /// </summary>
        void Remove(string serverHost);

        /// <summary>
        /// 取出客户端
        /// </summary>
        ClientConnectVO ToInfo(string serverHost);

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        bool IsExists(string serverHost);
    }
}