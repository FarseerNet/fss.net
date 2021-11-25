using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.Scheduler
{
    public interface ICheckClientOffline: ISingletonDependency
    {
        /// <summary>
        /// 检查客户端是否离线
        /// </summary>
        Task Check(TaskVO task);
    }
}