using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService
{
    /// <summary>
    /// 开启任务组调度
    /// </summary>
    public class StartTaskGroupScheduler : BackgroundService
    {
        private readonly IIocManager _ioc;

        public StartTaskGroupScheduler(IIocManager ioc)
        {
            _ioc = ioc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var taskGroupList      = _ioc.Resolve<ITaskGroupList>();
            var taskGroupScheduler = _ioc.Resolve<ITaskGroupScheduler>();
            var logger             = _ioc.Logger<Startup>();
            logger.LogInformation($"服务启动完成，监听 http://{IPAddress.Any}:80 ");
            
            // 遍历任务组、开启调度线程
            logger.LogInformation($"正在读取所有任务组信息");
            var taskGroupVos  = taskGroupList.ToList();
            logger.LogInformation($"共获取到：{taskGroupVos.Count} 条任务组信息");
            foreach (var taskGroupVo in taskGroupVos)
            {
                taskGroupScheduler.SchedulerTaskGroup(taskGroupVo.Id);
            }
            
        }
    }
}