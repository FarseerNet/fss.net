using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Com.SchedulerServer.Abstract
{
    public interface ICheckClientOffline: ITransientDependency
    {
        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        Task<bool> Check(TaskGroupVO taskGroupVO);
    }
}