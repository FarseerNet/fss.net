using System.Collections.Generic;
using System.Threading.Tasks;
using FS.DI;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Domain.Tasks.TaskGroup.Interface
{
    public interface ITaskGroupService: ISingletonDependency
    {
        /// <summary>
        /// 删除任务组
        /// </summary>
        Task DeleteAsync(int taskGroupId);
        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        Task<List<TaskGroupDO>> ToListAsync();
    }
}