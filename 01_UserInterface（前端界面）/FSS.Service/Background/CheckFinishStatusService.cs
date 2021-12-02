using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FSS.Abstract.Server.MetaInfo;

namespace FSS.Service.Background
{
    /// <summary>
    /// 检测完成状态的任务
    /// </summary>
    public class CheckFinishStatusService : BackgroundServiceTrace
    {
        public ITaskGroupList TaskGroupList { get; set; }
        public ITaskInfo      TaskInfo      { get; set; }
        public ITaskAdd       TaskAdd       { get; set; }

        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 取出任务组
                var dicTaskGroup = await TaskGroupList.ToListInCacheAsync();

                // 只检测Enable状态的任务组
                foreach (var taskGroupVO in dicTaskGroup.Where(o => o.IsEnable))
                {
                    var task = await TaskInfo.ToInfoByGroupIdAsync(taskGroupVO.Id);
                    if (task != null)
                    {
                        // 状态必须是 完成的
                        if (task.Status != FSS.Abstract.Enum.EumTaskType.Fail && task.Status != FSS.Abstract.Enum.EumTaskType.Success) continue;
                        // 加个时间，来限制并发
                        if ((DateTime.Now - task.RunAt).TotalSeconds < 3) continue;
                    }

                    await TaskAdd.GetOrCreateAsync(taskGroupVO.Id);
                    await Task.Delay(200, stoppingToken);
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}