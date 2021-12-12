using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;
using FSS.Infrastructure.Repository.TaskGroup;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository
{
    public class TaskGroupRepository : ITaskGroupRepository
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }
        public TaskGroupCache TaskGroupCache { get; set; }
        public TaskAgent      TaskAgent      { get; set; }
        public TaskCache      TaskCache      { get; set; }


        public Task SaveAsync(TaskGroupDO taskGroupDO) => TaskGroupCache.SaveAsync(taskGroupDO.Map<TaskGroupPO>());

        public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupCache.ToEntityAsync(EumCacheStoreType.Redis, taskGroupId).MapAsync<TaskGroupDO, TaskGroupPO>();

        public Task<List<TaskGroupDO>> ToListAsync() => TaskGroupCache.ToListAsync(EumCacheStoreType.Redis).MapAsync<TaskGroupDO, TaskGroupPO>();

        public Task<long> GetTaskGroupCountAsync() => TaskGroupCache.CountAsync();

        public async Task<List<TaskGroupDO>> ToListAsync(long clientId)
        {
            var lstTask = await TaskGroupCache.ToListAsync(EumCacheStoreType.Redis).MapAsync<TaskGroupDO, TaskGroupPO>();
            return lstTask.FindAll(o => o.Task.ClientId == clientId && o.StartAt < DateTime.Now);
        }

        public async Task<int> AddAsync(TaskGroupDO taskGroupDO)
        {
            var taskGroupId = await TaskGroupAgent.AddAsync(taskGroupDO.Map<TaskGroupPO>());
            await TaskGroupCache.ToEntityAsync(EumCacheStoreType.Redis, taskGroupId);
            return taskGroupId;
        }

        public async Task DeleteAsync(int taskGroupId)
        {
            await TaskGroupAgent.DeleteAsync(taskGroupId);
            await CacheKeys.TaskGroupClear(taskGroupId);
        }




        public Task<int> TodayFailCountAsync() => TaskAgent.TodayFailCountAsync();

        public Task<List<long>> ToTaskSpeedListAsync(int taskGroupId) => TaskAgent.ToSpeedListAsync(taskGroupId);

        public Task<List<TaskDO>> ToFinishListAsync(int taskGroupId, int top) => TaskAgent.ToFinishListAsync(taskGroupId, top).MapAsync<TaskDO, TaskPO>();

        public Task AddTaskAsync(TaskDO taskDO) => TaskCache.AddQueueAsync(taskDO.Map<TaskPO>());

        public async Task SyncToData()
        {
            var lst = await TaskGroupCache.ToListAsync(EumCacheStoreType.Redis);
            foreach (var taskGroupPO in lst)
            {
                await TaskGroupAgent.UpdateAsync(taskGroupPO.Id.GetValueOrDefault(), taskGroupPO);
            }
        }

        /// <summary>
        /// 获取所有任务组中的任务
        /// </summary>
        public async Task<List<TaskGroupDO>> GetMyCanSchedulerTaskGroup(string[] jobs, TimeSpan ts, int count)
        {
            var lstTaskGroup = await ToListAsync();
            return lstTaskGroup.Where(o => o.IsEnable && jobs.Contains(o.JobName) && o.StartAt < DateTime.Now.Add(ts) && o.Task is { Status: EumTaskType.None }).OrderBy(o => o.StartAt).Take(count).ToList();
        }

        /// <summary>
        /// 获取未执行的任务数量
        /// </summary>
        public async Task<int> ToUnRunCountAsync()
        {
            var lst = await ToListAsync();
            return lst.Count(o => o.StartAt < DateTime.Now && (o.Task == null || o.Task.Status == EumTaskType.None || o.Task.Status == EumTaskType.Scheduler));
        }

        /// <summary>
        /// 获取执行中的任务
        /// </summary>
        public async Task<List<TaskGroupDO>> ToSchedulerWorkingListAsync()
        {
            var lst = await ToListAsync();
            return lst.Where(o => o.Task != null && o.Task.Status == EumTaskType.Scheduler || o.Task.Status == EumTaskType.Working).ToList();
        }

        /// <summary>
        /// 获取进行中的任务
        /// </summary>
        public async Task<List<TaskGroupDO>> GetTaskUnFinishList(int top)
        {
            var lst = await ToListAsync();
            return lst.Where(o => o.Task != null && o.Task.Status != EumTaskType.Success && o.Task.Status != EumTaskType.Fail).OrderBy(o => o.StartAt).Take(top).ToList();
        }

        /// <summary>
        /// 获取指定任务组的任务列表（FOPS）
        /// </summary>
        public Task<List<TaskDO>> ToListAsync(int groupId, int pageSize, int pageIndex, out int totalCount)
        {
            return TaskAgent.ToListAsync(groupId, pageSize, pageIndex, out totalCount).MapAsync<TaskDO, TaskPO>();
        }

        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        public Task<List<TaskDO>> ToFinishListAsync(int pageSize, int pageIndex, out int totalCount)
        {
            return TaskAgent.ToFinishListAsync(pageSize, pageIndex, out totalCount).MapAsync<TaskDO, TaskPO>();
        }

        /// <summary>
        /// 获取在用的任务组
        /// </summary>
        public List<TaskGroupDO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex, out int totalCount)
        {
            var lst = TaskGroupCache.ToList(EumCacheStoreType.Redis).Where(o => o.IsEnable == true).Map<TaskGroupDO>();

            if (status.HasValue) lst = lst.Where(o => o.Task.Status == status.GetValueOrDefault()).ToList();
            totalCount = lst.Count;
            lst        = lst.OrderBy(o => o.JobName).ToList(pageSize, pageIndex);
            return lst;
        }
    }
}