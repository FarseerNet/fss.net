using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.Utils.Common;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 打印系统信息
    /// </summary>
    public class PrintSysInfoService : BackgroundService
    {
        private readonly IIocManager    _ioc;
        readonly         ILogger        _logger;
        readonly         ITaskGroupList _taskGroupList;

        public PrintSysInfoService(IIocManager ioc)
        {
            _ioc           = ioc;
            _logger        = _ioc.Logger<Startup>();
            _taskGroupList = _ioc.Resolve<ITaskGroupList>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ip = IpHelper.GetIps()[0].Address.MapToIPv4().ToString();
            _logger.LogInformation($"服务({ip})启动完成，监听 http://{IPAddress.Any}:80 ");

            var reservedTaskCount = _ioc.Resolve<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
            _logger.LogInformation($"当前系统设置任务至少保留：{reservedTaskCount}条");

            var configurationSection = _ioc.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:0:Server").Value;
            var use                  = !string.IsNullOrWhiteSpace(configurationSection) ? $"Elasticsearch，{configurationSection}" : "数据库";
            _logger.LogInformation($"使用 [ {use} ] 作为日志保存记录");

            _logger.LogInformation($"正在读取所有任务组信息");
            var taskGroupVos = await _taskGroupList.ToListByMemoryAsync();
            _logger.LogInformation($"共获取到：{taskGroupVos.Count} 条任务组信息");

            foreach (var taskGroupVo in taskGroupVos)
            {
                _logger.LogInformation($"【{taskGroupVo.Value.IsEnable}】{taskGroupVo.Value.Id}：{taskGroupVo.Value.Caption}、{taskGroupVo.Value.JobName}、{taskGroupVo.Value.NextAt:yyyy-MM-dd:HH:mm:ss}");
            }
            
            await _ioc.Resolve<IWhenTaskStatus>("None").Run();
            await _ioc.Resolve<IWhenTaskStatus>("Working").Run();
            await _ioc.Resolve<IWhenTaskStatus>("Finish").Run();
        }
    }
}