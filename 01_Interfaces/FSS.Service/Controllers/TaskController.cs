using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Core.Net;
using FS.DI;
using FSS.Application.Log.TaskLog.Interface;
using FSS.Application.Tasks.TaskGroup.Entity;
using FSS.Application.Tasks.TaskGroup.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EumTaskType = FSS.Domain.Tasks.TaskGroup.Enum.EumTaskType;

namespace FSS.Service.Controllers
{
    /// <summary>
    /// 任务相关信息
    /// </summary>
    [ApiController]
    [Route("task")]
    public class TaskController : BaseController
    {
        public IIocManager   IocManager   { get; set; }
        public ITaskLogApp   TaskLogApp   { get; set; }
        public ITaskGroupApp TaskGroupApp { get; set; }

        public TaskController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        /// <summary>
        /// 客户端拉取任务
        /// </summary>
        [HttpPost]
        [Route("Pull")]
        public async Task<ApiResponseJson<List<TaskDTO>>> Pull(PullDTO dto)
        {
            // 拉取任务
            var lstTask = await TaskGroupApp.TaskSchedulerAsync(Client, dto.TaskCount) ?? new List<TaskDTO>();
            return await ApiResponseJson<List<TaskDTO>>.SuccessAsync("默认", lstTask);
        }

        /// <summary>
        /// 客户端执行任务
        /// </summary>
        [HttpPost]
        [Route("JobInvoke")]
        public async Task<ApiResponseJson> JobInvoke(JobInvokeDTO dto)
        {
            var taskGroup = await TaskGroupApp.ToEntityAsync(dto.TaskGroupId);

            if (taskGroup == null)
            {
                await TaskLogApp.AddAsync(dto.TaskGroupId, "", "", LogLevel.Warning, $"所属的任务组：{dto.TaskGroupId} 不存在");
                return await ApiResponseJson.ErrorAsync($"所属的任务组：{dto.TaskGroupId} 不存在");
            }

            try
            {
                // 如果有日志
                if (dto.Log != null && !string.IsNullOrWhiteSpace(dto.Log.Log))
                {
                    await TaskLogApp.AddAsync(taskGroup, dto.Log.LogLevel, dto.Log.Log);
                }
                
                // 不相等，说明被覆盖了（JOB请求慢了。被调度重新执行了）
                if (taskGroup.Task.ClientId != Client.Id)
                {
                    await TaskLogApp.AddAsync(taskGroup, LogLevel.Warning, $"任务： {taskGroup.Caption}（{taskGroup.JobName}） ，{taskGroup.Task.ClientId}与本次请求的客户端{Client.Id} 不一致，忽略本次请求");
                    return await ApiResponseJson.ErrorAsync($"任务： {taskGroup.Caption}（{taskGroup.JobName}） ，{taskGroup.Task.ClientId}与本次请求的客户端{Client.Id} 不一致，忽略本次请求");
                }

                // 更新执行中状态
                await taskGroup.Working(dto.Data, dto.NextTimespan, dto.Progress, dto.Status, dto.RunSpeed);

                switch (taskGroup.Task.Status)
                {
                    case EumTaskType.Working:
                        return await ApiResponseJson.SuccessAsync($"任务组：TaskGroupId={taskGroup.Id}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName}，TaskId={taskGroup.Task.Id} 更新成功");
                    case EumTaskType.Success:
                        return await ApiResponseJson.SuccessAsync($"任务组：TaskGroupId={taskGroup.Id}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName}，TaskId={taskGroup.Task.Id} 执行成功，耗时：{taskGroup.Task.RunSpeed} ms");
                    default:
                        var message = $"任务组：TaskGroupId={taskGroup.Id}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName}，TaskId={taskGroup.Task.Id} 执行失败";
                        await TaskLogApp.AddAsync(taskGroup, LogLevel.Warning, message);
                        return await ApiResponseJson.ErrorAsync(message);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                await taskGroup.CancelTask();
                await TaskLogApp.AddAsync(taskGroup, LogLevel.Error, e.Message);
                return await ApiResponseJson.ErrorAsync(e.Message);
            }
        }
    }
}