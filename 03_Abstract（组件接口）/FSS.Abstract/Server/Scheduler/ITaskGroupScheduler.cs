namespace FSS.Abstract.Server.Scheduler
{
    public interface ITaskGroupScheduler
    {
        /// <summary>
        /// 根据任务组ID，进行任务调度
        /// </summary>
        /// <param name="taskGroupId">任务组ID</param>
        void Scheduler(int taskGroupId);
    }
}