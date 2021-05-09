using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using FSS.Com.SchedulerServer.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 检查所有调度逻辑是否仍在运行
    /// </summary>
    public class CheckSchedulerWorkingService : BackgroundService
    {
        private readonly IIocManager     _ioc;
        readonly         ILogger         _logger;
        private readonly IClientRegister _clientRegister;

        public CheckSchedulerWorkingService(IIocManager ioc)
        {
            _ioc            = ioc;
            _logger         = _ioc.Logger<Startup>();
            _clientRegister = _ioc.Resolve<IClientRegister>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                if (_clientRegister.Count() > 0)
                {
                    if (!WhenTaskStatusNone.IsRun || !WhenTaskStatusWorking.IsRun || !WhenTaskStatusFinish.IsRun)
                    {
                        await _ioc.Resolve<IWhenTaskStatus>("None").Run();
                        await _ioc.Resolve<IWhenTaskStatus>("Working").Run();
                        await _ioc.Resolve<IWhenTaskStatus>("Finish").Run();
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}