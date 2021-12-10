using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Application.Tasks.TaskGroup.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks.Interface;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskGroupApp : ITaskGroupApp
    {
        public ITaskGroupService TaskGroupService { get; set; }
        public ITaskGroupAgent   TaskGroupAgent   { get; set; }
        public ITaskAgent        TaskAgent        { get; set; }

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

        /// <summary>
        /// 今日执行失败数量
        /// </summary>
        public Task<int> TodayFailCountAsync() => TaskAgent.TodayFailCountAsync();

        /// <summary>
        /// 计算任务的平均运行速度
        /// </summary>
        public async Task<long> StatAvgSpeedAsync(int taskGroupId)
        {
            var speedList = await TaskAgent.ToSpeedListAsync(taskGroupId);
            if (speedList.Count == 0) return 0;
            return speedList.Sum() / speedList.Count;
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public Task<List<TaskGroupDO>> ToListAsync() => TaskGroupService.ToListAsync();
    }
}