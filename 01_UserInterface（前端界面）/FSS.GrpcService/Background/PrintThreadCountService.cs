using System.Diagnostics;
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
    /// 打印当前线程数量
    /// </summary>
    public class PrintThreadCountService : BackgroundService
    {
        private readonly IIocManager         _ioc;
        readonly         ITaskGroupList      _taskGroupList;
        readonly         ITaskGroupScheduler _taskGroupScheduler;
        readonly         ILogger             _logger;

        public PrintThreadCountService(IIocManager ioc)
        {
            _ioc                = ioc;
            _taskGroupList      = _ioc.Resolve<ITaskGroupList>();
            _taskGroupScheduler = _ioc.Resolve<ITaskGroupScheduler>();
            _logger             = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                var threadsCount = Process.GetCurrentProcess().Threads.Count;
                _logger.LogInformation($"当前线程数量：{threadsCount}");
                await Task.Delay(1000);
            }
        }
    }
}