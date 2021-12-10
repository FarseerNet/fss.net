using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Application.Tasks.TaskGroup.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Model;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskGroupApp : ITaskGroupApp
    {
        public ITaskGroupService TaskGroupService { get; set; }
        public ITaskGroupAgent   TaskGroupAgent   { get; set; }

        /// <summary>
        /// 添加任务组信息
        /// </summary>
        public Task<int> AddAsync(TaskGroupDTO dto) => dto.Map<TaskGroupDO>().AddAsync();

        /// <summary>
        /// 取消任务
        /// </summary>
        public async Task CancelTask(int groupId)
        {
            var taskGroup = await TaskGroupAgent.ToEntityAsync(EumCacheStoreType.Redis, groupId).MapAsync<TaskGroupDO, TaskGroupPO>();
            await taskGroup.CancelTask();
        }
    }
}