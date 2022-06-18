using System;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.Exception;
using FS.Core.LinkTrack;
using FSS.Domain.Client.Clients.Repository;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Tasks.TaskGroup;
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
            using var lstTask = await TaskGroupRepository.ToSchedulerWorkingListAsync();
            foreach (var taskGroupDO in lstTask)
            {
                await CheckTaskGroup(taskGroup: taskGroupDO);
                await Task.Delay(millisecondsDelay: 200, cancellationToken: stoppingToken);
            }
            await Task.Delay(delay: TimeSpan.FromSeconds(value: 30), cancellationToken: stoppingToken);
        }
    }

    private async Task CheckTaskGroup(TaskGroupDO taskGroup)
    {
        try
        {
            taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId: taskGroup.Id);
            if (taskGroup.Task == null)
            {
                taskGroup.CreateTask();
                TaskGroupRepository.Save(taskGroup);
                return;
            }

            // 任务不存在
            if (taskGroup.Task != null && taskGroup.Task.Client.Id > 0)
            {
                var client = ClientRepository.ToEntity(clientId: taskGroup.Task.Client.Id);
                if (client == null) throw new RefuseException(message: $"【客户端不存在】{taskGroup.Task.Client.Id}，强制下线客户端");
            }

            // 检查任务开启状态
            taskGroup.CheckClientOffline();
        }
        catch (RefuseException e)
        {
            TaskLogService.Add(taskGroupId: taskGroup.Id, jobName: taskGroup.JobName, caption: taskGroup.Caption, logLevel: LogLevel.Warning, content: e.Message);
            taskGroup.Cancel();
            TaskGroupRepository.Save(taskGroup);
        }
    }
}