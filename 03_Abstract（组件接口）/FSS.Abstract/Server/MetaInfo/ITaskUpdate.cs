using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskUpdate: ITransientDependency
    {
        /// <summary>
        /// 更新Task（如果状态是成功、失败、重新调度，则应该调Save）
        /// </summary>
        Task UpdateAsync(TaskVO task);

        /// <summary>
        /// 保存Task
        /// </summary>
        Task SaveFinishAsync(TaskVO task, TaskGroupVO taskGroup);

        /// <summary>
        /// 保存Task
        /// </summary>
        Task SaveAsync(TaskVO task);

        /// <summary>
        /// 移除缓存的任务
        /// </summary>
        Task RemoveAsync(int taskGroupId);

        /// <summary>
        /// 保存Task（taskGroup必须是最新的）
        /// </summary>
        Task SaveFinishAsync(TaskVO task);

        /// <summary>
        /// 移除缓存
        /// </summary>
        Task ClearCacheAsync();
    }
}