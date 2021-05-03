using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupUpdate: ITransientDependency
    {
        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        Task UpdateAsync(TaskGroupVO taskGroup);

        /// <summary>
        /// 更新任务ID
        /// </summary>
        Task UpdateTaskIdAsync(int taskGroupId, int taskId);

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        Task SaveAsync(TaskGroupVO taskGroup);

        /// <summary>
        /// 统计失败次数，按次数递增时间
        /// </summary>
        Task StatFailAsync(TaskVO task);
    }
}