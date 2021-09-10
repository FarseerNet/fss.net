using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Background
{
    /// <summary>
    /// 打印当前线程数量
    /// </summary>
    public class PrintThreadCountService : BackgroundService
    {
        private readonly IIocManager         _ioc;
        readonly         ILogger             _logger;

        public PrintThreadCountService(IIocManager ioc)
        {
            _ioc                = ioc;
            _logger             = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                var threadsCount = Process.GetCurrentProcess().Threads.Count;
                _logger.LogInformation($"当前线程数量：{threadsCount}");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}