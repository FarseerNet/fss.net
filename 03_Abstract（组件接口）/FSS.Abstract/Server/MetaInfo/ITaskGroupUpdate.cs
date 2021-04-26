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
    }
}