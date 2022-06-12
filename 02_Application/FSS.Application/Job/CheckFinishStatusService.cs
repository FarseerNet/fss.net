using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Application.Job;

/// <summary>
///     检测完成状态的任务
/// </summary>
public class CheckFinishStatusService : BackgroundServiceTrace
{
    public TaskGroupService     TaskGroupService    { get; set; }
    public ITaskGroupRepository TaskGroupRepository { get; set; }

    protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 取出任务组
            var dicTaskGroup = await TaskGroupService.ToListAsync();

            // 只检测Enable状态的任务组
            foreach (var taskGroupId in dicTaskGroup.Where(predicate: o => o.IsEnable).Select(o => o.Id))
            {
                var taskGroup = await TaskGroupRepository.ToEntityAsync(taskGroupId);

                // 加个时间，来限制并发
                if ((DateTime.Now - taskGroup.Task.RunAt).TotalSeconds < 30) continue;

                taskGroup.CreateTask();
                await Task.Delay(millisecondsDelay: 200, cancellationToken: stoppingToken);
            }
            await Task.Delay(delay: TimeSpan.FromSeconds(value: 30), cancellationToken: stoppingToken);
        }
    }
}