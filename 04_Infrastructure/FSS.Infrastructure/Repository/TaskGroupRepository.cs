using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Cache.Attribute;
using FS.Core.Abstract.Data;
using FS.Core.Abstract.MQ.Queue;
using FS.Core.AOP.Data;
using FS.DI;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;
using FSS.Infrastructure.Repository.Context;
using FSS.Infrastructure.Repository.TaskGroup;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository;

public class TaskGroupRepository : ITaskGroupRepository
{
    private const string cacheKey = "FSS_TaskGroup";

    public TaskGroupAgent TaskGroupAgent { get; set; }
    public TaskAgent      TaskAgent      { get; set; }


    [Cache(cacheKey)]
    public Task<PooledList<TaskGroupDO>> ToListAsync() => TaskGroupAgent.ToListAsync().MapAsync<TaskGroupDO, TaskGroupPO>();

    [Cache(cacheKey)]
    public List<TaskGroupDO> ToList() => TaskGroupAgent.ToList().Map<TaskGroupDO>();

    [CacheUpdate(cacheKey)]
    public TaskGroupDO Save(TaskGroupDO taskGroupDO) => taskGroupDO; // 直接返回，是因为暂时不直接更新到数据库，减少数据库IO

    [CacheCount(cacheKey)]
    public Task<int> GetTaskGroupCountAsync() => TaskGroupAgent.Count();

    [CacheRemove(cacheKey)]
    [Transaction(typeof(MysqlContext))]
    public async Task DeleteAsync(int taskGroupId)
    {
        await TaskGroupAgent.DeleteAsync(taskGroupId: taskGroupId);
        await TaskAgent.DeleteAsync(taskGroupId: taskGroupId);
    }

    [CacheItem(cacheKey)]
    public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupAgent.ToEntityAsync(taskGroupId).MapAsync<TaskGroupDO, TaskGroupPO>();

    [CacheUpdate(cacheKey)]
    public async Task<TaskGroupDO> AddAsync(TaskGroupDO taskGroupDO)
    {
        var taskGroupId = await TaskGroupAgent.AddAsync(po: taskGroupDO);
        taskGroupDO.Id = taskGroupId;
        return taskGroupDO;
    }

    public async Task<PooledList<TaskGroupDO>> ToListAsync(long clientId)
    {
        var lstTask = await ToListAsync();
        return lstTask.FindAll(match: o => o.Task != null && o.Task.Client != null && o.Task.Client.ClientId == clientId && o.Task.StartAt < DateTime.Now);
    }

    public Task<int> TodayFailCountAsync() => TaskAgent.TodayFailCountAsync();

    public Task<PooledList<long>> ToTaskSpeedListAsync(int taskGroupId) => TaskAgent.ToSpeedListAsync(taskGroupId: taskGroupId);

    public Task<PooledList<TaskDO>> ToFinishListAsync(int taskGroupId, int top) => TaskAgent.ToFinishListAsync(groupId: taskGroupId, top: top).MapAsync(mapRule: TaskPO.MapToDO);

    public void AddTask(TaskDO taskDO) => IocManager.GetService<IQueueProduct>(name: "TaskQueue").Send(taskDO.Map<TaskPO>());

    public async Task SyncToData()
    {
        var lst = await ToListAsync();
        foreach (var taskGroupPO in lst) await TaskGroupAgent.UpdateAsync(id: taskGroupPO.Id, taskGroup: taskGroupPO);
    }

    /// <summary>
    ///     获取能调度的任务
    /// </summary>
    public async Task<PooledList<TaskDO>> GetCanSchedulerTaskGroup(string[] jobsName, TimeSpan ts, int count, ClientVO client)
    {
        using (var locker = RedisContext.Instance.GetLocker("FSS_Scheduler", TimeSpan.FromSeconds(5)))
        {
            if (!locker.TryLock()) return new PooledList<TaskDO>();

            var lstTaskGroup = await ToListAsync();
            lstTaskGroup = lstTaskGroup.Where(predicate: o => o.IsEnable && jobsName.Contains(value: o.JobName) && o.Task != null && o.Task.Status == EumTaskType.None && o.Task.StartAt < DateTime.Now.Add(value: ts) && o.Task.Client?.ClientId == 0).OrderBy(keySelector: o => o.Task.StartAt).Take(count: count).ToPooledList();

            var lst = new PooledList<TaskDO>();
            foreach (var taskGroupDO in lstTaskGroup)
            {
                // 设为调度状态
                taskGroupDO.SchedulerAsync(client: client);

                // 如果不相等，说明被其它客户端拿了
                if (taskGroupDO.Task.Client.ClientId == client.ClientId) lst.Add(item: taskGroupDO.Task);
            }
            return lst;
        }
    }

    /// <summary>
    ///     获取进行中的任务
    /// </summary>
    public async Task<List<TaskGroupDO>> GetTaskUnFinishList(IEnumerable<string> jobsName, int top)
    {
        var lstTaskGroup = await ToListAsync();
        return lstTaskGroup.Where(predicate: o => o.IsEnable && jobsName.Contains(o.JobName) && o.Task != null && o.Task.Status != EumTaskType.Success && o.Task.Status != EumTaskType.Fail).OrderBy(keySelector: o => o.Task.StartAt).Take(count: top).ToList();
    }

    /// <summary>
    ///     获取执行中的任务
    /// </summary>
    public async Task<List<TaskGroupDO>> ToSchedulerWorkingListAsync()
    {
        var lst = await ToListAsync();
        return lst.Where(predicate: o => o.Task is { Status: EumTaskType.Scheduler or EumTaskType.Working }).ToList();
    }

    /// <summary>
    ///     获取未执行的任务数量
    /// </summary>
    public async Task<int> ToUnRunCountAsync()
    {
        var lst = await ToListAsync();
        return lst.Count(predicate: o => o.Task == null || o.Task.Status is EumTaskType.None or EumTaskType.Scheduler || o.Task.CreateAt < DateTime.Now);
    }

    /// <summary>
    ///     获取指定任务组的任务列表（FOPS）
    /// </summary>
    public Task<PageList<TaskDO>> ToListAsync(int groupId, int pageSize, int pageIndex) => TaskAgent.ToListAsync(groupId: groupId, pageSize: pageSize, pageIndex: pageIndex).MapAsync(mapRule: TaskPO.MapToDO);

    /// <summary>
    ///     获取已完成的任务列表
    /// </summary>
    public Task<PageList<TaskDO>> ToFinishPageListAsync(int pageSize, int pageIndex) => TaskAgent.ToFinishPageListAsync(pageSize: pageSize, pageIndex: pageIndex).MapAsync(mapRule: TaskPO.MapToDO);

    /// <summary>
    ///     获取在用的任务组
    /// </summary>
    public PageList<TaskDO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex)
    {
        var lst = ToList().Where(predicate: o => o.IsEnable).ToPooledList();

        if (status.HasValue) lst = lst.Where(predicate: o => o.Task.Status == status.GetValueOrDefault()).ToPooledList();
        var totalCount           = lst.Count;
        lst = lst.OrderBy(keySelector: o => o.JobName).ToList(pageSize: pageSize, pageIndex: pageIndex);

        return new PageList<TaskDO>(lst.Select(o => o.Task).ToPooledList(), totalCount);
    }
}