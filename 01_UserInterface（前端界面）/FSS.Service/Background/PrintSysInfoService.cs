using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FS.Cache;
using FS.DI;
using FS.Extends;
using FS.Utils.Common;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Background
{
    /// <summary>
    /// 打印系统信息
    /// </summary>
    public class PrintSysInfoService : BackgroundService
    {
        private readonly IIocManager    _ioc;
        readonly         ILogger        _logger;
        readonly         ITaskGroupList _taskGroupList;
        readonly         ITaskGroupInfo _taskGroupInfo;

        public PrintSysInfoService(IIocManager ioc)
        {
            _ioc           = ioc;
            _logger        = _ioc.Logger<Startup>();
            _taskGroupList = _ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo = _ioc.Resolve<ITaskGroupInfo>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ip = IpHelper.GetIps()[0].Address.MapToIPv4().ToString();
            
            _logger.LogInformation($"服务({ip})启动完成，监听 {_ioc.Resolve<IConfigurationRoot>().GetSection("Kestrel:Endpoints:Http:Url").Value} ");

            var reservedTaskCount = _ioc.Resolve<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
            _logger.LogInformation($"当前系统设置任务至少保留：{reservedTaskCount}条");

            var configurationSection = _ioc.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:0:Server").Value;
            var use                  = !string.IsNullOrWhiteSpace(configurationSection) ? $"Elasticsearch，{configurationSection}" : "数据库";
            _logger.LogInformation($"使用 [ {use} ] 作为日志保存记录");

            _logger.LogInformation($"正在读取所有任务组信息");
            var lstGroupByDb = await _taskGroupList.ToListInDbAsync();
            var lstGroupByCache = await _taskGroupList.ToListInMemoryAsync();
            _logger.LogInformation($"共获取到：{lstGroupByCache.Count} 条任务组信息");

            foreach (var taskGroupVo in lstGroupByDb)
            {
                // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
                await _taskGroupInfo.ToInfoAsync(taskGroupVo.Id);
                
                _logger.LogInformation($"【{taskGroupVo.IsEnable}】{taskGroupVo.Id}：{taskGroupVo.Caption}、{taskGroupVo.JobName}、{taskGroupVo.NextAt:yyyy-MM-dd:HH:mm:ss}");
            }
            
            await _ioc.Resolve<IWhenTaskStatus>("Working").Run();
            await _ioc.Resolve<IWhenTaskStatus>("Finish").Run();
        }
    }
}