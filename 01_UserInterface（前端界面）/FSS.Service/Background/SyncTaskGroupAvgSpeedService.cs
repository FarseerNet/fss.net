using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Background
{
    /// <summary>
    /// 计算任务组的平均耗时
    /// </summary>
    public class SyncTaskGroupAvgSpeedService : BackgroundService
    {
        private readonly IIocManager      _ioc;
        readonly         ITaskGroupList   _taskGroupList;
        readonly         ITaskGroupInfo   _taskGroupInfo;
        readonly         ITaskGroupUpdate _taskGroupUpdate;
        readonly         ITaskInfo        _taskInfo;
        readonly         ILogger          _logger;

        public SyncTaskGroupAvgSpeedService(IIocManager ioc)
        {
            _ioc             = ioc;
            _taskGroupList   = _ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo   = _ioc.Resolve<ITaskGroupInfo>();
            _taskGroupUpdate = _ioc.Resolve<ITaskGroupUpdate>();
            _taskInfo        = _ioc.Resolve<ITaskInfo>();
            _logger          = _ioc.Logger<SyncTaskGroupAvgSpeedService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await Task.Delay(5 * 60 * 1000, stoppingToken);
                var taskGroupVos = await _taskGroupList.ToListInCacheAsync();
                foreach (var taskGroupVo in taskGroupVos)
                {
                    // 先计算在更新
                    var statAvgSpeed = await _taskInfo.StatAvgSpeedAsync(taskGroupVo.Id);
                    if (statAvgSpeed == 0) continue;
                    
                    var taskGroupVO = await _taskGroupInfo.ToInfoAsync(taskGroupVo.Id);
                    taskGroupVO.RunSpeedAvg = statAvgSpeed;
                    await _taskGroupUpdate.SaveAsync(taskGroupVO);
                    //_logger.LogDebug($"成功同步TaskGroupId={taskGroupVO.Id} 的平均耗时为：{statAvgSpeed} ms");
                    
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}