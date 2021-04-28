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
            await Task.Delay(2000);
            _ioc.Logger<Startup>().LogInformation("遍历任务组、开启调度线程");
            // 遍历任务组、开启调度线程
            var taskGroupVos = _ioc.Resolve<ITaskGroupList>().ToList();
            foreach (var taskGroupVo in taskGroupVos)
            {
                _ioc.Resolve<ITaskGroupScheduler>().SchedulerTaskGroup(taskGroupVo.Id);
            }
        }
    }
}