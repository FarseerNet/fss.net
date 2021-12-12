using System;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FSS.Application.Clients.Interface;
using FSS.Application.Log.TaskLog.Interface;
using FSS.Application.Tasks.TaskGroup.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Background
{
    /// <summary>
    /// 检测进行中状态的任务
    /// </summary>
    public class CheckWorkStatusService : BackgroundServiceTrace
    {
        public ITaskGroupApp TaskGroupApp { get; set; }
        public IClientApp    ClientApp    { get; set; }
        public ITaskLogApp   TaskLogApp   { get; set; }

        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 取出任务组
                var lstTask = await TaskGroupApp.ToSchedulerWorkingListAsync();
                foreach (var taskGroupDO in lstTask)
                {
                    if (await CheckTaskGroup(taskGroupDO: taskGroupDO)) continue;
                    await Task.Delay(200, stoppingToken);
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        private async Task<bool> CheckTaskGroup(TaskGroupDO taskGroupDO)
        {
            try
            {
                // 任务不存在
                if (taskGroupDO.Task == null)
                {
                    await taskGroupDO.CreateTask();
                    return true;
                }

                var client = await ClientApp.ToEntityAsync(taskGroupDO.Task.ClientId);
                if (client == null) throw new Exception($"【客户端不存在】{taskGroupDO.Task.ClientId}，强制下线客户端");

                // 检查任务开启状态
                await taskGroupDO.CheckClientOffline();
            }
            catch (Exception e)
            {
                await TaskLogApp.AddAsync(taskGroupDO.Id, taskGroupDO.JobName, taskGroupDO.Caption, LogLevel.Warning, e.Message);
                await taskGroupDO.CancelTask();
            }
            return false;
        }
    }
}