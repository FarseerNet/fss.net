﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Collections.Pooled;
using FS.Core.Extend;
using FS.Core.Net;
using FSS.Application.Log.TaskLog;
using FSS.Application.Tasks.TaskGroup;
using FSS.Application.Tasks.TaskGroup.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Controllers;

/// <summary>
///     任务相关信息
/// </summary>
[ApiController]
[Route(template: "task")]
public class TaskController : BaseController
{
    public TaskController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor: httpContextAccessor)
    {
    }
    public TaskLogApp       TaskLogApp       { get; set; }
    public TaskProcessApp   TaskProcessApp   { get; set; }
    public TaskQueryApp     TaskQueryApp     { get; set; }
    public TaskSchedulerApp TaskSchedulerApp { get; set; }

    // /// <summary>
    // ///     客户端拉取任务
    // /// </summary>
    // [HttpPost]
    // [Route(template: "Pull")]
    // public async Task<ApiResponseJson<PooledList<TaskDTO>>> Pull(PullDTO dto)
    // {
    //     // 拉取任务
    //     var taskScheduler = await TaskSchedulerApp.TaskSchedulerAsync(client: Client, requestTaskCount: dto.TaskCount) ?? new();
    //     return taskScheduler.ToSuccess();
    // }

    // /// <summary>
    // ///     客户端执行任务
    // /// </summary>
    // [HttpPost]
    // [Route(template: "JobInvoke")]
    // public async Task<ApiResponseJson> JobInvoke(JobInvokeDTO dto)
    // {
    //     var taskGroup = await TaskQueryApp.ToEntityAsync(taskGroupId: dto.TaskGroupId);
    //     if (taskGroup == null)
    //     {
    //         TaskLogApp.Add(taskGroupId: dto.TaskGroupId, jobName: "", caption: "", logLevel: LogLevel.Warning, content: $"所属的任务组：{dto.TaskGroupId} 不存在");
    //         return await ApiResponseJson.ErrorAsync(statusMessage: $"所属的任务组：{dto.TaskGroupId} 不存在");
    //     }
    //
    //     var statusMessage = $"任务组：TaskGroupId={dto.TaskGroupId}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName} 处理成功";
    //     return await TaskProcessApp.JobInvoke(dto: dto, taskGroup: taskGroup, client: Client).ToSuccessAsync(statusMessage);
    // }
}