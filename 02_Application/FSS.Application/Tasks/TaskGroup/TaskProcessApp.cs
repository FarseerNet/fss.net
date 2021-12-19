using System;
using System.Threading.Tasks;
using FS.Core.Exception;
using FS.DI;
using FSS.Application.Clients.Client.Entity;
using FSS.Application.Log.TaskLog;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Domain.Log.TaskLog;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using Microsoft.Extensions.Logging;

namespace FSS.Application.Tasks.TaskGroup
{
    public class TaskProcessApp : ISingletonDependency
    {
        public TaskLogService TaskLogService { get; set; }
        public TaskLogApp      TaskLogApp     { get; set; }

        /// <summary>
        /// 客户端执行任务
        /// </summary>
        public async Task JobInvoke(JobInvokeDTO dto, TaskGroupDO taskGroup, ClientDTO client)
        {
            try
            {
                if (taskGroup == null) throw new RefuseException($"所属的任务组：{dto.TaskGroupId} 不存在");

                // 如果有日志
                if (dto.Log != null && !string.IsNullOrWhiteSpace(dto.Log.Log))
                {
                    await TaskLogApp.AddAsync(taskGroup, dto.Log.LogLevel, dto.Log.Log);
                }

                // 不相等，说明被覆盖了（JOB请求慢了。被调度重新执行了）
                if (taskGroup.Task.Client.ClientId != client.Id)
                {
                    throw new RefuseException($"任务： {taskGroup.Caption}（{taskGroup.JobName}） ，{taskGroup.Task.Client.ClientId}与本次请求的客户端{client.Id} 不一致，忽略本次请求");
                }

                // 更新执行中状态
                await taskGroup.Working(dto.Data, dto.NextTimespan, dto.Progress, dto.Status, dto.RunSpeed);

                if (dto.Status is not (EumTaskType.Working or EumTaskType.Success))
                {
                    throw new RefuseException($"任务组：TaskGroupId={taskGroup.Id}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName} 执行失败");
                }
            }
            catch (RefuseException e)
            {
                if (taskGroup != null) await TaskLogService.AddAsync(dto.TaskGroupId, taskGroup.JobName, taskGroup.Caption, LogLevel.Warning, e.Message);
                throw;
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                if (taskGroup != null)
                {
                    await taskGroup.CancelAsync();
                    await TaskLogService.AddAsync(taskGroup.Id, taskGroup.JobName, taskGroup.Caption, LogLevel.Error, e.ToString());
                }
                throw e;
            }
        }
    }
}