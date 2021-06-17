using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Entity.MetaInfo;

namespace FSS.Abstract.Server.MetaInfo
{
    public interface ITaskGroupCache: ITransientDependency
    {
        /// <summary>
        /// 保存任务组信息
        /// </summary>
        Task SaveAsync(int taskGroupId, TaskGroupVO taskGroup);

        /// <summary>
        /// 当前任务组的列表
        /// </summary>
        Task<List<TaskGroupVO>> ToListAsync();

        /// <summary>
        /// 获取任务组
        /// </summary>
        Task<TaskGroupVO> ToEntityAsync(int taskGroupId);

        /// <summary>
        /// 移除缓存任务组ID
        /// </summary>
        Task RemoveAsync(int taskGroupId);
    }
}