using FS.DI;

namespace FSS.Com.RegisterCenterServer.Abstract
{
    public interface IClientEndpointAdd: ITransientDependency
    {
        /// <summary>
        /// 添加客户端信息到客户端列表
        /// </summary>
        void Add(string clientId, string endpoint);
    }
}