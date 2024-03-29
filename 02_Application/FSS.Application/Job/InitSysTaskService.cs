using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core.LinkTrack;
using FS.DI;
using FSS.Application.Tasks.TaskGroup;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Job;

/// <summary>
///     初始化系统任务
/// </summary>
public class InitSysTaskService : BackgroundServiceTrace
{
    private readonly ILogger              _logger;
    private readonly ITaskGroupRepository _taskGroupRepository;
    private readonly TaskQueryApp         _taskQueryApp;
    private readonly TaskGroupApp         TaskGroupApp;

    public InitSysTaskService(IIocManager ioc)
    {
        _logger              = ioc.Logger<InitSysTaskService>();
        _taskQueryApp        = ioc.Resolve<TaskQueryApp>();
        TaskGroupApp         = ioc.Resolve<TaskGroupApp>();
        _taskGroupRepository = ioc.Resolve<ITaskGroupRepository>();
    }

    protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(message: "正在读取所有任务组信息");
        using var lstTaskGroup = await _taskQueryApp.ToListAsync();

        // 检查是否存在系统任务组
        await CreateSysJob(lstTaskGroup: lstTaskGroup, jobName: "FSS.ClearHisTask", caption: "清除历史任务", intervalMs: TimeSpan.FromHours(value: 1));
        await CreateSysJob(lstTaskGroup: lstTaskGroup, jobName: "FSS.SyncTaskGroupAvgSpeed", caption: "计算任务平均耗时", intervalMs: TimeSpan.FromHours(value: 1));
        await CreateSysJob(lstTaskGroup: lstTaskGroup, jobName: "FSS.SyncTaskGroup", caption: "同步任务组缓存", intervalMs: TimeSpan.FromMinutes(value: 1));
        await CreateSysJob(lstTaskGroup: lstTaskGroup, jobName: "FSS.CheckClientOffline", caption: "检查超时离线的客户端", intervalMs: TimeSpan.FromMinutes(value: 1));

        _logger.LogInformation(message: $"共获取到：{lstTaskGroup.Count} 条任务组信息");
        foreach (var taskGroupVo in lstTaskGroup)
        // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
            await _taskGroupRepository.ToEntityAsync(taskGroupId: taskGroupVo.Id);
    }

    /// <summary>
    ///     创建系统任务
    /// </summary>
    private async Task CreateSysJob(PooledList<TaskGroupDO> lstTaskGroup, string jobName, string caption, TimeSpan intervalMs, Dictionary<string, string> data = null)
    {
        if (lstTaskGroup.All(predicate: o => o.JobName != jobName))
        {
            var taskGroupDTO = new TaskGroupDTO
            {
                Caption = caption,
                JobName = jobName,
                Data    = data ?? new Dictionary<string, string>(), Cron = $"{(int)intervalMs.TotalMilliseconds}", StartAt = DateTime.Now, NextAt = DateTime.Now, ActivateAt = DateTime.Now, LastRunAt = DateTime.Now, IsEnable = true
            };

            taskGroupDTO.Id = await TaskGroupApp.AddAsync(taskGroupDTO);
            lstTaskGroup.Add(item: taskGroupDTO);
        }

        foreach (var taskGroupVo in lstTaskGroup.FindAll(match: o => o.JobName == jobName && o.IsEnable == false))
        {
            var taskGroup = await _taskGroupRepository.ToEntityAsync(taskGroupId: taskGroupVo.Id);
            if (taskGroup == null) throw new Exception(message: "任务组不存在");
            
            taskGroup.SetEnable(enable: true);
            _taskGroupRepository.Save(taskGroup);
            taskGroupVo.IsEnable = true;
        }
    }
}