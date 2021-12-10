using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Application.Tasks.Tasks.Interface
{
    public interface ITaskApp: ISingletonDependency
    {
        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        Task<TaskDO> GetOrCreateAsync(int taskGroupId);
        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        Task<List<TaskDO>> ToGroupListAsync();
    }
}