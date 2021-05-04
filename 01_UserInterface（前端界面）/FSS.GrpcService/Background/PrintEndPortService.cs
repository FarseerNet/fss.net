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
    /// 打印当前监听
    /// </summary>
    public class PrintEndPortService : BackgroundService
    {
        private readonly IIocManager         _ioc;
        readonly         ILogger             _logger;

        public PrintEndPortService(IIocManager ioc)
        {
            _ioc                = ioc;
            _logger             = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ip = IpHelper.GetIps()[0].Address.MapToIPv4().ToString();
            _logger.LogInformation($"服务({ip})启动完成，监听 http://{IPAddress.Any}:80 ");
        }
    }
}