using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.RegisterCenter;
using Microsoft.Extensions.Hosting;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 同步当前信息到缓存
    /// </summary>
    public class SyncServiceInfoService : BackgroundService
    {
        private readonly IIocManager     _ioc;
        readonly         INodeRegister   _nodeRegister;
        readonly         IClientRegister _clientRegister;

        public SyncServiceInfoService(IIocManager ioc)
        {
            _ioc            = ioc;
            _nodeRegister = _ioc.Resolve<INodeRegister>();
            _clientRegister = _ioc.Resolve<IClientRegister>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                // 每1S，更新当前IP到服务列表中
                _nodeRegister.Register();
                _clientRegister.SyncCache();
                
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}