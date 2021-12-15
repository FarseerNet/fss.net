using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FS.DI;
using FS.ElasticSearch.Configuration;
using FS.Extends;
using FS.Utils.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Background
{
    /// <summary>
    /// 打印系统信息
    /// </summary>
    public class PrintSysInfoService : BackgroundServiceTrace
    {
        private readonly IIocManager _ioc;
        readonly         ILogger     _logger;

        public PrintSysInfoService(IIocManager ioc)
        {
            _ioc    = ioc;
            _logger = _ioc.Logger<Startup>();
        }

        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            var ip = IpHelper.GetIps()[0].Address.MapToIPv4().ToString();

            _logger.LogInformation($"服务({ip})启动完成，监听 {_ioc.Resolve<IConfigurationRoot>().GetSection("Kestrel:Endpoints:Http:Url").Value} ");

            var reservedTaskCount = _ioc.Resolve<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
            _logger.LogInformation($"当前系统设置任务至少保留：{reservedTaskCount}条");

            var elasticSearchItemConfig = ElasticSearchConfigRoot.Get().Find(o => o.Name == "es");
            var use                     = elasticSearchItemConfig != null ? $"Elasticsearch，{elasticSearchItemConfig.Server}" : "数据库";
            _logger.LogInformation($"使用 [ {use} ] 作为日志保存记录");
        }

    }
}