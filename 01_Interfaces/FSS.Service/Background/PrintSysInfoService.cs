using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FS.DI;
using FS.ElasticSearch.Configuration;
using FS.Extends;
using FS.Job;
using FS.Job.Entity;
using FS.Utils.Common;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Application.Tasks.TaskGroup.Interface;
using FSS.Application.Tasks.Tasks.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FSS.Service.Background
{
    /// <summary>
    /// 打印系统信息
    /// </summary>
    public class PrintSysInfoService : BackgroundServiceTrace
    {
        private readonly IIocManager      _ioc;
        readonly         ILogger          _logger;
        readonly         ITaskGroupList   _taskGroupList;
        readonly         ITaskGroupInfo   _taskGroupInfo;
        readonly         ITaskGroupApp    _taskGroupApp;
        readonly         ITaskAdd         _taskAdd;
        readonly         ITaskApp         _taskApp;
        readonly         ITaskGroupUpdate _taskGroupUpdate;

        public PrintSysInfoService(IIocManager ioc)
        {
            _ioc              = ioc;
            _logger           = _ioc.Logger<Startup>();
            _taskGroupList    = _ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo    = _ioc.Resolve<ITaskGroupInfo>();
            _taskGroupApp = _ioc.Resolve<ITaskGroupApp>();
            _taskAdd          = _ioc.Resolve<ITaskAdd>();
            _taskApp          = _ioc.Resolve<ITaskApp>();
            _taskGroupUpdate  = _ioc.Resolve<ITaskGroupUpdate>();
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

            _logger.LogInformation($"正在读取所有任务组信息");
            var lstTaskGroup = await _taskGroupList.ToListInCacheAsync();

            // 检查是否存在系统任务组
            await CreateSysJob(lstTaskGroup, "FSS.ClearHisTask", "清除历史任务", TimeSpan.FromHours(1));
            await CreateSysJob(lstTaskGroup, "FSS.SyncTaskGroupAvgSpeed", "计算任务平均耗时", TimeSpan.FromHours(1));
            await CreateSysJob(lstTaskGroup, "FSS.SyncTaskGroup", "同步任务组缓存", TimeSpan.FromMinutes(1));
            await CreateSysJob(lstTaskGroup, "FSS.AddTaskToDb", "任务写入数据库", TimeSpan.FromMinutes(1), JsonConvert.SerializeObject(new { DataCount    = 100 }));
            await CreateSysJob(lstTaskGroup, "FSS.AddRunLogToDb", "日志写入数据库", TimeSpan.FromSeconds(10), JsonConvert.SerializeObject(new { DataCount = 100 }));

            _logger.LogInformation($"共获取到：{lstTaskGroup.Count} 条任务组信息");
            foreach (var taskGroupVo in lstTaskGroup)
            {
                // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
                await _taskGroupInfo.ToInfoAsync(taskGroupVo.Id);
                _logger.LogInformation($"【{taskGroupVo.IsEnable}】{taskGroupVo.Id}：{taskGroupVo.Caption}、{taskGroupVo.JobName}、{taskGroupVo.NextAt:yyyy-MM-dd:HH:mm:ss}");
            }
        }

        private async Task CreateSysJob(List<TaskGroupDTO> lstTaskGroup, string jobName, string caption, TimeSpan intervalMs, string data = null)
        {
            if (lstTaskGroup.All(o => o.JobName != jobName))
            {
                var taskGroupVO = new TaskGroupDTO
                {
                    Caption = caption,
                    JobName = jobName,
                    Data    = data ?? "{}", Cron = $"{(int)intervalMs.TotalMilliseconds}", StartAt = DateTime.Now, NextAt = DateTime.Now, ActivateAt = DateTime.Now, LastRunAt = DateTime.Now, IsEnable = true
                };
                taskGroupVO.Id = await _taskGroupApp.AddAsync(taskGroupVO);
                await _taskApp.GetOrCreateAsync(taskGroupVO.Id);
                lstTaskGroup.Add(taskGroupVO);
            }

            foreach (var taskGroupVo in lstTaskGroup)
            {
                if (taskGroupVo.JobName == jobName)
                {
                    if (!taskGroupVo.IsEnable)
                    {
                        taskGroupVo.IsEnable = true;
                        await _taskGroupUpdate.UpdateAsync(taskGroupVo);
                    }
                    await _taskApp.GetOrCreateAsync(taskGroupVo);
                }
            }
        }
    }
}