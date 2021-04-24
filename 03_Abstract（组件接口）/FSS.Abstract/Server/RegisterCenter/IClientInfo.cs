using FS.DI;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Abstract.Server.RegisterCenter
{
    public interface IClientInfo: ITransientDependency
    {
        /// <summary>
        /// 通过轮询的方式，取出客户端
        /// </summary>
        ClientVO Slb();
    }
}