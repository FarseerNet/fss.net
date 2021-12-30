using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FSS.Domain.Tasks.TaskGroup;

namespace FSS.Application.Job
{
    /// <summary>
    /// 检测完成状态的任务
    /// </summary>
    public class CheckFinishStatusService : BackgroundServiceTrace
    {
        public TaskGroupService TaskGroupService { get; set; }

        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 取出任务组
                var dicTaskGroup = await TaskGroupService.ToListAsync();

                // 只检测Enable状态的任务组
                foreach (var taskGroupDO in dicTaskGroup.Where(o => o.IsEnable))
                {
                    // 加个时间，来限制并发
                    if ((DateTime.Now - taskGroupDO.Task.RunAt).TotalSeconds < 30) continue;

                    await taskGroupDO.CreateTask();
                    await Task.Delay(200, stoppingToken);
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}