using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Utils.Common;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 开启任务组调度
    /// </summary>
    [Obsolete]
    public class RunThreadSchedulerService : BackgroundService
    {
        private readonly IIocManager         _ioc;
        readonly         ITaskGroupList      _taskGroupList;
        readonly         ITaskGroupScheduler _taskGroupScheduler;
        readonly         ILogger             _logger;

        public RunThreadSchedulerService(IIocManager ioc)
        {
            _ioc                = ioc;
            _taskGroupList      = _ioc.Resolve<ITaskGroupList>();
            _taskGroupScheduler = _ioc.Resolve<ITaskGroupScheduler>();
            _logger             = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 遍历任务组、开启调度线程
            _logger.LogInformation($"正在读取所有任务组信息");
            var taskGroupVos = await _taskGroupList.ToListAndSaveAsync();

            _logger.LogInformation($"共获取到：{taskGroupVos.Count} 条任务组信息");
            foreach (var taskGroupVo in taskGroupVos)
            {
                _taskGroupScheduler.SchedulerTaskGroup(taskGroupVo.Id);
            }
        }
    }
}