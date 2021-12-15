using System;
using System.Threading.Tasks;
using FS.Core.Net;
using FS.DI;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Log.TaskLog;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Client.Clients.Entity;
using FSS.Domain.Log.TaskLog.Interface;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Repository;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskProcessApp : ISingletonDependency
    {
        public ITaskLogService      TaskLogService      { get; set; }
        public TaskLogApp           TaskLogApp          { get; set; }

        /// <summary>
        /// 客户端执行任务
        /// </summary>
        public async Task JobInvoke(JobInvokeDTO dto, TaskGroupDO taskGroup, ClientDTO client)
        {
            if (taskGroup == null)
            {
                await TaskLogService.AddAsync(dto.TaskGroupId, "", "", LogLevel.Warning, $"所属的任务组：{dto.TaskGroupId} 不存在");
                throw new Exception($"所属的任务组：{dto.TaskGroupId} 不存在");
            }

            try
            {
                // 如果有日志
                if (dto.Log != null && !string.IsNullOrWhiteSpace(dto.Log.Log))
                {
                    await TaskLogApp.AddAsync(taskGroup, dto.Log.LogLevel, dto.Log.Log);
                }

                // 不相等，说明被覆盖了（JOB请求慢了。被调度重新执行了）
                if (taskGroup.Task.Client.ClientId != client.Id)
                {
                    await TaskLogApp.AddAsync(taskGroup, LogLevel.Warning, $"任务： {taskGroup.Caption}（{taskGroup.JobName}） ，{taskGroup.Task.Client.ClientId}与本次请求的客户端{client.Id} 不一致，忽略本次请求");
                    throw new Exception($"任务： {taskGroup.Caption}（{taskGroup.JobName}） ，{taskGroup.Task.Client.ClientId}与本次请求的客户端{client.Id} 不一致，忽略本次请求");
                }

                // 更新执行中状态
                await taskGroup.Working(dto.Data, dto.NextTimespan, dto.Progress, dto.Status, dto.RunSpeed);

                if (taskGroup.Task.Status is not (EumTaskType.Working or EumTaskType.Success))
                {
                    var message = $"任务组：TaskGroupId={taskGroup.Id}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName} 执行失败";
                    await TaskLogApp.AddAsync(taskGroup, LogLevel.Warning, message);
                    throw new Exception(message);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                await taskGroup.CancelAsync();
                await TaskLogApp.AddAsync(taskGroup, LogLevel.Error, e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}