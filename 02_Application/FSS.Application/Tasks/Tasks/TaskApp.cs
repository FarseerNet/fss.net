using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Application.Tasks.Tasks.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Interface;

namespace FSS.Application.Tasks.Tasks
{
    public class TaskApp : ITaskApp
    {
        public ITaskGroupAgent   TaskGroupAgent   { get; set; }
        public ITaskGroupService TaskGroupService { get; set; }

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        public async Task<TaskDO> GetOrCreateAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupAgent.ToEntityAsync(EumCacheStoreType.Redis, taskGroupId);
            return await TaskGroupService.CreateTaskAsync(taskGroup.Map<TaskGroupDO>());
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskDO>> ToGroupListAsync() => TaskGroupService.ToGroupListAsync();
    }
}