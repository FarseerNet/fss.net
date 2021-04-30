using System;
using System.Collections.Generic;
using FS.DI;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientRegister: ITransientDependency
    {
        /// <summary>
        /// 取出客户端列表
        /// </summary>
        List<ClientConnectVO> ToList();

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        void UpdateUseAt(string serverHost, DateTime useAt);

        /// <summary>
        /// 更新客户端心跳时间
        /// </summary>
        void UpdateHeartbeatAt(string serverHost, DateTime heartbeatAt);

        /// <summary>
        /// 同步本地缓存到Redis
        /// </summary>
        void SyncCache();

        /// <summary>
        /// 注册
        /// </summary>
        void Register(ClientConnectVO client);

        /// <summary>
        /// 取出客户端
        /// </summary>
        ClientConnectVO ToInfo(string serverHost);

        /// <summary>
        /// 移除客户端
        /// </summary>
        void Remove(string serverHost);

        /// <summary>
        /// 获取客户端数量
        /// </summary>
        int Count(string jobName);
    }
}