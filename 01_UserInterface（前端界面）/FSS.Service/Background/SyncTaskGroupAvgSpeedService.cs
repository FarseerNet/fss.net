using System;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Background
{
    /// <summary>
    /// 计算任务组的平均耗时
    /// </summary>
    public class SyncTaskGroupAvgSpeedService : LoopService
    {
        readonly ITaskGroupList   _taskGroupList;
        readonly ITaskGroupInfo   _taskGroupInfo;
        readonly ITaskGroupUpdate _taskGroupUpdate;
        readonly ITaskInfo        _taskInfo;

        public SyncTaskGroupAvgSpeedService(IIocManager ioc)
        {
            _taskGroupList   = ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo   = ioc.Resolve<ITaskGroupInfo>();
            _taskGroupUpdate = ioc.Resolve<ITaskGroupUpdate>();
            _taskInfo        = ioc.Resolve<ITaskInfo>();
        }

        protected override TimeSpan SleepMs { get; set; } = TimeSpan.FromHours(1);
        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            var taskGroupVos = await _taskGroupList.ToListInCacheAsync();
            foreach (var taskGroupVo in taskGroupVos)
            {
                // 先计算在更新
                var statAvgSpeed = await _taskInfo.StatAvgSpeedAsync(taskGroupVo.Id);
                if (statAvgSpeed == 0) continue;

                var taskGroupVO = await _taskGroupInfo.ToInfoAsync(taskGroupVo.Id);
                taskGroupVO.RunSpeedAvg = statAvgSpeed;
                await _taskGroupUpdate.SaveAsync(taskGroupVO);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}