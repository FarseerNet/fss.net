using System;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.Exception;
using FS.Core.LinkTrack;
using FSS.Domain.Client.Clients.Repository;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Job;

/// <summary>
///     检测进行中状态的任务
/// </summary>
public class CheckWorkStatusService : BackgroundServiceTrace
{
    public ITaskGroupRepository TaskGroupRepository { get; set; }
    public IClientRepository    ClientRepository    { get; set; }
    public TaskLogService       TaskLogService      { get; set; }

    protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 取出任务组
            var lstTask = await TaskGroupRepository.ToSchedulerWorkingListAsync();
            foreach (var taskGroupDO in lstTask)
            {
                if (await CheckTaskGroup(taskGroupDO: taskGroupDO)) continue;
                await Task.Delay(millisecondsDelay: 200, cancellationToken: stoppingToken);
            }
            await Task.Delay(delay: TimeSpan.FromSeconds(value: 30), cancellationToken: stoppingToken);
        }
    }

    private async Task<bool> CheckTaskGroup(TaskGroupDO taskGroupDO)
    {
        try
        {
            taskGroupDO = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroupDO.Id);
            // 任务不存在
            if (taskGroupDO.Task != null && taskGroupDO.Task.Client.ClientId > 0)
            {
                var client = await ClientRepository.ToEntityAsync(clientId: taskGroupDO.Task.Client.ClientId);
                if (client == null) throw new RefuseException(message: $"【客户端不存在】{taskGroupDO.Task.Client.ClientId}，强制下线客户端");
            }

            // 检查任务开启状态
            await taskGroupDO.CheckClientOffline();
        }
        catch (RefuseException e)
        {
            TaskLogService.Add(taskGroupId: taskGroupDO.Id, jobName: taskGroupDO.JobName, caption: taskGroupDO.Caption, logLevel: LogLevel.Warning, content: e.Message);
            await taskGroupDO.CancelAsync();
        }
        return false;
    }
}