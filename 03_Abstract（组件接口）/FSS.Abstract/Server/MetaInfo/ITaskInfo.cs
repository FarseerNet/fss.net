using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskInfo: ITransientDependency
    {
        /// <summary>
        /// 获取任务信息
        /// </summary>
        TaskVO ToInfo(int id);

        /// <summary>
        /// 获取当前任务组的任务
        /// </summary>
        TaskVO ToGroupTask(int taskGroupId);

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        int StatAvgSpeed(int taskGroupId);
    }
}