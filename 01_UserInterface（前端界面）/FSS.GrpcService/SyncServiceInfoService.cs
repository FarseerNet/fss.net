using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Utils.Common;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService
{
    /// <summary>
    /// 同步当前信息到缓存
    /// </summary>
    public class SyncServiceInfoService : BackgroundService
    {
        private readonly IIocManager     _ioc;
        readonly         IServerRegister _serverRegister;
        readonly         ILogger         _logger;

        public SyncServiceInfoService(IIocManager ioc)
        {
            _ioc            = ioc;
            _serverRegister = _ioc.Resolve<IServerRegister>();
            _logger         = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                // 每1S，更新当前IP到服务列表中
                _serverRegister.Register();
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}