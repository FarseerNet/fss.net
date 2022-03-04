using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core;
using FS.Extends;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;
using FSS.Infrastructure.Repository.TaskGroup;
using FSS.Infrastructure.Repository.Tasks;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository;

public class TaskGroupRepository : ITaskGroupRepository
{
    public TaskGroupAgent TaskGroupAgent { get; set; }
    public TaskGroupCache TaskGroupCache { get; set; }
    public TaskAgent      TaskAgent      { get; set; }
    public TaskCache      TaskCache      { get; set; }


    public Task SaveAsync(TaskGroupDO taskGroupDO) => TaskGroupCache.SaveAsync(taskGroup: taskGroupDO);

    public Task<TaskGroupDO> ToEntityAsync(int taskGroupId) => TaskGroupCache.ToEntityAsync(taskGroupId: taskGroupId);

    public Task<List<TaskGroupDO>> ToListAsync() => TaskGroupCache.ToListAsync();

    public Task<long> GetTaskGroupCountAsync() => TaskGroupCache.CountAsync();

    public async Task<List<TaskGroupDO>> ToListAsync(long clientId)
    {
        var lstTask = await TaskGroupCache.ToListAsync();
        return lstTask.FindAll(match: o => o.Task != null && o.Task.Client != null && o.Task.Client.ClientId == clientId && o.Task.StartAt < DateTime.Now);
    }

    public async Task<int> AddAsync(TaskGroupDO taskGroupDO)
    {
        var taskGroupId = await TaskGroupAgent.AddAsync(po: taskGroupDO);
        await TaskGroupCache.ToEntityAsync(taskGroupId: taskGroupId);
        return taskGroupId;
    }

    public async Task DeleteAsync(int taskGroupId)
    {
        await TaskGroupAgent.DeleteAsync(taskGroupId: taskGroupId);
        await TaskAgent.DeleteAsync(taskGroupId: taskGroupId);
        await TaskGroupCache.TaskGroupClear(taskGroupId: taskGroupId);
    }

    public Task<int> TodayFailCountAsync() => TaskAgent.TodayFailCountAsync();

    public Task<List<long>> ToTaskSpeedListAsync(int taskGroupId) => TaskAgent.ToSpeedListAsync(taskGroupId: taskGroupId);

    public Task<List<TaskDO>> ToFinishListAsync(int taskGroupId, int top) => TaskAgent.ToFinishListAsync(groupId: taskGroupId, top: top).MapAsync(mapRule: TaskPO.MapToDO);

    public Task AddTaskAsync(TaskDO taskDO) => TaskCache.AddQueueAsync(task: taskDO);

    public async Task SyncToData()
    {
        var lst = await TaskGroupCache.ToListAsync();
        foreach (var taskGroupPO in lst) await TaskGroupAgent.UpdateAsync(id: taskGroupPO.Id, taskGroup: taskGroupPO);
    }

    /// <summary>
    ///     获取能调度的任务
    /// </summary>
    public async Task<List<TaskGroupDO>> GetCanSchedulerTaskGroup(string[] jobsName, TimeSpan ts, int count)
    {
        var lstTaskGroup = await ToListAsync();
        return lstTaskGroup.Where(predicate: o => o.IsEnable && jobsName.Contains(value: o.JobName) && o.Task != null && o.Task.Status == EumTaskType.None && o.Task.StartAt < DateTime.Now.Add(value: ts)).OrderBy(keySelector: o => o.Task.StartAt).Take(count: count).ToList();
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
    public List<TaskGroupDO> GetEnableTaskList(EumTaskType? status, int pageSize, int pageIndex, out int totalCount)
    {
        var lst = TaskGroupCache.ToList().Where(predicate: o => o.IsEnable).ToList();

        if (status.HasValue) lst = lst.Where(predicate: o => o.Task.Status == status.GetValueOrDefault()).ToList();
        totalCount = lst.Count;
        lst        = lst.OrderBy(keySelector: o => o.JobName).ToList(pageSize: pageSize, pageIndex: pageIndex);
        return lst;
    }
}