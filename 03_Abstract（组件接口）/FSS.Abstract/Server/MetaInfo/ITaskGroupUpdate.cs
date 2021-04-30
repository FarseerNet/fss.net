using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupUpdate: ITransientDependency
    {
        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        void Update(TaskGroupVO taskGroup);

        /// <summary>
        /// 更新任务ID
        /// </summary>
        void UpdateTaskId(int taskGroupId, int taskId);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        void Save(TaskGroupVO taskGroup);

        /// <summary>
        /// 统计失败次数，按次数递增时间
        /// </summary>
        void StatFail(TaskVO task);
    }
}