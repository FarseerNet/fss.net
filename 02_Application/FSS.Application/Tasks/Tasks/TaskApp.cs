using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Application.Tasks.Tasks.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks.Interface;

namespace FSS.Application.Tasks.Tasks
{
    public class TaskApp : ITaskApp
    {
        public ITaskGroupAgent   TaskGroupAgent   { get; set; }
        public ITaskGroupService TaskGroupService { get; set; }
        public ITaskAgent        TaskAgent        { get; set; }

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        public async Task<TaskDO> GetOrCreateAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupAgent.ToEntityAsync(EumCacheStoreType.Redis, taskGroupId).MapAsync<TaskGroupDO,TaskGroupPO>();
            await taskGroup.CreateTask();
            return taskGroup.Task;
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskGroupDO>> ToGroupListAsync() => TaskGroupService.ToListAsync();

        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        public async Task<int> AddToDbAsync(int top)
        {
            var lstTask = await TaskAgent.GetFinishTaskListAsync(top);
            if (lstTask.Count == 0) return 0;

            return await TaskAgent.AddToDbAsync(lstTask);
        }
    }
}