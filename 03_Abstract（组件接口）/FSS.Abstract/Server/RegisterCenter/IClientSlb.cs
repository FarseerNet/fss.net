using System;
using FS.DI;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientSlb: ITransientDependency
    {
        /// <summary>
        /// 通过轮询的方式，取出客户端
        /// </summary>
        ClientVO Slb();

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        void UpdateUseAt(string clientId, DateTime useAt);

        /// <summary>
        /// 下线客户端
        /// </summary>
        void Remove(string clientId);
    }
}