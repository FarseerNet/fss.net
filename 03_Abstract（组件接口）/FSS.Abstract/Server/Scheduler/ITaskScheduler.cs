using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.Scheduler
{
    public interface ITaskScheduler: ITransientDependency
    {
        /// <summary>
        /// 调度
        /// </summary>
        Task Scheduler(TaskGroupVO taskGroup, TaskVO task);
    }
}