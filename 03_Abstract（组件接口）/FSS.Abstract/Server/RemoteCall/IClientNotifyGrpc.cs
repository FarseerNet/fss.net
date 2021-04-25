using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;

namespace FSS.Abstract.Server.RemoteCall
{
    public interface IClientNotifyGrpc: ITransientDependency
    {
        /// <summary>
        /// 远程通知客户端执行JOB
        /// </summary>
        Task<TaskVO> Invoke(ClientVO client, TaskVO task);
    }
}