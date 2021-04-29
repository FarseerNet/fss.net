using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Utils.Common;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService
{
    /// <summary>
    /// 开启任务组调度
    /// </summary>
    public class TaskGroupSchedulerService : BackgroundService
    {
        private readonly IIocManager         _ioc;
        readonly         ITaskGroupList      _taskGroupList;
        readonly         ITaskGroupScheduler _taskGroupScheduler;
        readonly         ILogger             _logger;

        public TaskGroupSchedulerService(IIocManager ioc)
        {
            _ioc                = ioc;
            _taskGroupList      = _ioc.Resolve<ITaskGroupList>();
            _taskGroupScheduler = _ioc.Resolve<ITaskGroupScheduler>();
            _logger             = _ioc.Logger<Startup>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ip = IpHelper.GetIps()[0].Address.MapToIPv4().ToString();
            _logger.LogInformation($"服务({ip})启动完成，监听 http://{IPAddress.Any}:80 ");

            // 遍历任务组、开启调度线程
            _logger.LogInformation($"正在读取所有任务组信息");
            var taskGroupVos = _taskGroupList.ToListAndSave();
            _logger.LogInformation($"共获取到：{taskGroupVos.Count} 条任务组信息");
            foreach (var taskGroupVo in taskGroupVos)
            {
                _taskGroupScheduler.SchedulerTaskGroup(taskGroupVo.Id);
            }

            return Task.FromResult(0);
        }
    }
}