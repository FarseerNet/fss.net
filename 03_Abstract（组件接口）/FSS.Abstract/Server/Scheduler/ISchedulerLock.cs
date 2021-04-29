using FS.DI;

namespace FSS.Abstract.Server.Scheduler
{
    public interface ISchedulerLock: ITransientDependency
    {
        /// <summary>
        /// 锁住任务，只允许一个节点对其调度
        /// </summary>
        bool TryLock(int taskId, string serverNode);
    }
}