using System.Collections.Generic;
using FS.DI;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Com.RegisterCenterServer.Abstract
{
    public interface IClientEndpoint: ITransientDependency
    {
        /// <summary>
        /// 添加客户端信息到客户端列表
        /// </summary>
        void Save(string clientId, ClientVO client);

        /// <summary>
        /// 获取客户端
        /// </summary>
        ClientVO ToEntity(string clientId);

        /// <summary>
        /// 当前注册的列表
        /// </summary>
        List<ClientVO> ToList();
    }
}