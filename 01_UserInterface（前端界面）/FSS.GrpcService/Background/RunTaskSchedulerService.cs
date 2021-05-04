using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 开启任务组调度
    /// </summary>
    public class RunTaskSchedulerService : BackgroundService
    {
        private readonly IIocManager         _ioc;
        readonly         ITaskGroupList      _taskGroupList;
        readonly         ITaskGroupScheduler _taskGroupScheduler;
        readonly         ILogger             _logger;
        readonly         ITaskInfo           _taskInfo;
        readonly         IWhenTaskStatus _whenTaskStatusNone;

        public RunTaskSchedulerService(IIocManager ioc)
        {
            _ioc                = ioc;
            _taskGroupList      = _ioc.Resolve<ITaskGroupList>();
            _taskGroupScheduler = _ioc.Resolve<ITaskGroupScheduler>();
            _taskInfo           = _ioc.Resolve<ITaskInfo>();
            _whenTaskStatusNone = _ioc.Resolve<IWhenTaskStatus>("None");
            _logger             = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 遍历任务组、开启调度线程
            _logger.LogInformation($"正在读取所有任务组信息");
            var taskGroupVos = await _taskGroupList.ToListAndSaveAsync();
            _logger.LogInformation($"共获取到：{taskGroupVos.Count} 条任务组信息");

            // 运行状态=Node的任务
            //await _whenTaskStatusNone.Run();
        }
    }
}