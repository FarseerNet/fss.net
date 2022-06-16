using System;
using System.Threading.Tasks;
using FS;
using FS.Core.Abstract.AspNetCore;
using FS.Core.Extend;
using FS.DI;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Tasks;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Tasks.TaskGroup;

[UseApi(Area = "meta")]
public class TaskGroupApp : ISingletonDependency
{
    public ITaskGroupRepository   TaskGroupRepository    { get; set; }
    public TaskGroupDeleteService TaskGroupDeleteService { get; set; }
    public TaskLogService         TaskLogService         { get; set; }

    /// <summary>
    ///     复制任务组
    /// </summary>
    [Api("CopyTaskGroup", HttpMethod.POST, "复制成功")]
    public async Task<int> CopyTaskGroupAsync(OnlyIdRequest request)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(request.Id);
        if (taskGroup == null) throw new Exception(message: "要复制的任务组不存在");

        var newTaskGroup = new TaskGroupDO(taskGroup);
        await TaskGroupRepository.AddAsync(newTaskGroup);

        return newTaskGroup.Id;
    }

    /// <summary>
    ///     删除任务组
    /// </summary>
    [Api("DeleteTaskGroup")]
    public Task DeleteAsync(OnlyIdRequest request) => TaskGroupDeleteService.DeleteAsync(request.Id).ToSuccessAsync("删除成功");

    /// <summary>
    ///     添加任务组信息
    /// </summary>
    [Api("AddTaskGroup")]
    public async Task<int> AddAsync(TaskGroupDTO dto)
    {
        if (dto.Caption == null || dto.Cron == null || dto.Data == null || dto.JobName == null) throw new FarseerException("标题、时间间隔、传输数据、Job名称 必须填写");
        TaskGroupDO taskGroup = dto;
        taskGroup.CheckInterval();
        await TaskGroupRepository.AddAsync(taskGroup);

        taskGroup.CreateTask();
        TaskGroupRepository.Save(taskGroup);
        return taskGroup.Id;
    }

    /// <summary>
    ///     保存任务组
    /// </summary>
    [Api("SaveTaskGroup")]
    public async Task Save(TaskGroupDTO dto)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: dto.Id);
        if (taskGroup == null) throw new Exception(message: "任务组不存在");

        taskGroup.Set(dto.JobName, dto.Caption, dto.Data, dto.StartAt);
        taskGroup.Set(dto.Cron, dto.IntervalMs);
        taskGroup.SetEnable(dto.IsEnable);
        TaskGroupRepository.Save(taskGroup);
    }

    /// <summary>
    ///     取消任务
    /// </summary>
    [Api("CancelTask")]
    public async Task CancelTask(OnlyIdRequest request)
    {
        var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: request.Id);
        taskGroup.Cancel();
        TaskGroupRepository.Save(taskGroup);

        TaskLogService.Add(taskGroupId: request.Id, jobName: taskGroup.JobName, caption: taskGroup.Caption, logLevel: LogLevel.Information, content: "手动取消任务");
    }

    /// <summary>
    ///     同步数据
    /// </summary>
    [Api("SyncCacheToDb")]
    public Task SyncTaskGroup() => TaskGroupRepository.SyncToData();
}