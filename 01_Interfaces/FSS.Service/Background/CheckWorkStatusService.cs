using System;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;

namespace FSS.Service.Background
{
    /// <summary>
    /// 检测进行中状态的任务
    /// </summary>
    public class CheckWorkStatusService : BackgroundServiceTrace
    {
        public ITaskList           TaskList           { get; set; }
        public ICheckClientOffline CheckClientOffline { get; set; }

        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 取出任务组
                var lstTask = await TaskList.ToSchedulerWorkingListAsync();

                foreach (var task in lstTask)
                {
                    // 检查是否离线
                    await CheckClientOffline.Check(task);
                    await Task.Delay(200, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

}