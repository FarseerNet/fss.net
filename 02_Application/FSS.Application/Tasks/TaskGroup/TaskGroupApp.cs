using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Tasks.TaskGroup;

public class TaskGroupApp : ISingletonDependency
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    public TaskLogService       TaskLogService      { get; set; }

    /// <summary>
    ///     添加任务组信息
    /// </summary>
    public Task<int> AddAsync(TaskGroupDTO dto) => dto.Map<TaskGroupDO>().AddAsync();

    /// <summary>
    ///     保存任务组
    /// </summary>
    public async Task Save(TaskGroupDTO dto)
    {
        var oldTaskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: dto.Id);
        if (oldTaskGroup == null) throw new Exception(message: "任务组不存在");

        await oldTaskGroup.SaveAsync(newTaskGroupDO: dto);
    }

    /// <summary>
    ///     取消任务
    /// </summary>
    public async Task CancelTask(int taskGroupId)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);
        await taskGroup.CancelAsync();
        TaskLogService.Add(taskGroupId: taskGroupId, jobName: taskGroup.JobName, caption: taskGroup.Caption, logLevel: LogLevel.Information, content: "手动取消任务");
    }

    /// <summary>
    ///     同步数据
    /// </summary>
    public Task SyncTaskGroup() => TaskGroupRepository.SyncToData();
    /// <summary>
    ///     复制任务组
    /// </summary>
    public async Task<int> CopyTaskGroup(int taskGroupId)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);
        if (taskGroup == null) throw new Exception(message: "要复制的任务组不存在");
        return await taskGroup.CopyAsync();
    }

    /// <summary>
    ///     删除任务组
    /// </summary>
    public async Task DeleteAsync(int taskGroupId)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupId);
        if (taskGroup == null) throw new Exception(message: "要删除的任务组不存在");
        await taskGroup.DeleteAsync();
    }
}