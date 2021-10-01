using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Cache;
using FS.Core.Net;
using FS.DI;
using FSS.Abstract.Entity;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FSS.Service.Controllers
{
    /// <summary>
    /// 任务相关信息
    /// </summary>
    [ApiController]
    [Route("task")]
    public class TaskController : BaseController
    {
        public ITaskList        TaskList        { get; set; }
        public IIocManager      IocManager      { get; set; }
        public IRunLogAdd       RunLogAdd       { get; set; }
        public ITaskInfo        TaskInfo        { get; set; }
        public ITaskGroupList   TaskGroupList   { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskUpdate      TaskUpdate      { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }

        public TaskController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        /// <summary>
        /// 客户端拉取任务
        /// </summary>
        [HttpPost]
        [Route("Pull")]
        public async Task<ApiResponseJson<List<TaskVO>>> Pull(PullRequest request)
        {
            // 拉取任务
            var lstTask = await TaskList.PullTaskAsync(Client, request.TaskCount) ?? new List<TaskVO>();
            return await ApiResponseJson<List<TaskVO>>.SuccessAsync("默认", lstTask);
        }

        /// <summary>
        /// 客户端拉取任务
        /// </summary>
        [HttpPost]
        [Route("JobInvoke")]
        public async Task<ApiResponseJson> JobInvoke(JobInvokeRequest request)
        {
            var logger = IocManager.Logger<TaskController>();
            var task   = await TaskInfo.ToInfoByGroupIdAsync(request.TaskGroupId);

            // 先读本地缓存
            var lstGroup = await TaskGroupList.ToListInMemoryAsync();
            lstGroup.TryGetValue(request.TaskGroupId, out var taskGroup);

            if (taskGroup == null)
            {
                await RunLogAdd.AddAsync(task.TaskGroupId, request.Id, LogLevel.Warning, $"所属的任务组：{task.TaskGroupId} 不存在");
                return await ApiResponseJson.ErrorAsync($"指定的TaskId：{request.Id} 所属的任务组：{task.TaskGroupId} 不存在");
            }

            // 服务端当前任务ID与客户端请求的任务ID不一致
            if (task.Id > request.Id)
            {
                await RunLogAdd.AddAsync(taskGroup, request.Id, LogLevel.Warning, $"请求的任务{request.Id}与服务端正在处理的Task（{task.Id}）： {taskGroup.Caption}（{taskGroup.JobName}） 不一致");
                return await ApiResponseJson.ErrorAsync($"指定的TaskId：{request.Id} {taskGroup.Caption}（{taskGroup.JobName}） 与服务端正在处理的Task：{task.Id} 不一致");
            }

            try
            {
                // 不相等，说明被覆盖了（JOB请求慢了。被调度重新执行了）
                if (task.ClientId != Client.Id)
                {
                    await RunLogAdd.AddAsync(taskGroup, request.Id, LogLevel.Warning, $"{task.ClientId}与本次请求{Client.Id} 不一致，忽略本次请求");
                    return await ApiResponseJson.ErrorAsync($"任务ID： {taskGroup.Caption}（{taskGroup.JobName}） ，{task.ClientId}与本次请求{Client.Id} 不一致，忽略本次请求");
                }

                // 更新Task元信息
                if (task.Status == EumTaskType.Scheduler)
                {
                    task.RunAt = DateTime.Now; // 首次执行，记录时间
                    // 更新group元信息
                    taskGroup = await TaskGroupInfo.ToInfoAsync(request.TaskGroupId);
                    taskGroup.RunCount++;
                    taskGroup.LastRunAt = DateTime.Now;
                }

                // 更新Task
                taskGroup.ActivateAt = DateTime.Now;
                task.Progress        = request.Progress;
                task.Status          = request.Status;
                task.RunSpeed        = request.RunSpeed;

                // 如果有日志
                if (request.Log != null && !string.IsNullOrWhiteSpace(request.Log.Log))
                {
                    await RunLogAdd.AddAsync(taskGroup, task.Id, request.Log.LogLevel, request.Log.Log);
                }

                // 如果是成功、错误状态，则要立即更新数据库
                switch (task.Status)
                {
                    case EumTaskType.Fail:
                    case EumTaskType.Success:
                    case EumTaskType.ReScheduler:
                        taskGroup.Data = JsonConvert.SerializeObject(request.Data);
                        // 客户端设置了动态时间
                        if (request.NextTimespan > 0)
                        {
                            taskGroup.NextAt = DateTime.Now.AddMilliseconds(request.NextTimespan);
                        }
                        await TaskUpdate.SaveFinishAsync(task, taskGroup);
                        break;
                    default:
                        await TaskUpdate.UpdateAsync(task);
                        await TaskGroupUpdate.UpdateAsync(taskGroup);
                        break;
                }

                var message = $"任务组：TaskGroupId={task.TaskGroupId}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName}，TaskId={task.Id} 执行失败";
                switch (task.Status)
                {
                    case EumTaskType.Working:
                        return await ApiResponseJson.SuccessAsync($"任务组：TaskGroupId={task.TaskGroupId}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName}，TaskId={task.Id} 更新成功");
                    case EumTaskType.Fail:
                        IocManager.Logger<TaskController>().LogInformation(message);
                        return await ApiResponseJson.ErrorAsync(message);
                    case EumTaskType.Success:
                        return await ApiResponseJson.SuccessAsync($"任务组：TaskGroupId={task.TaskGroupId}，Caption={taskGroup.Caption}，JobName={taskGroup.JobName}，TaskId={task.Id} 执行成功，耗时：{task.RunSpeed} ms");
                    default:
                        IocManager.Logger<TaskController>().LogInformation(message);
                        return await ApiResponseJson.ErrorAsync(message);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                task.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(task, taskGroup);
                logger.LogError(e, e.Message);
                return await ApiResponseJson.ErrorAsync(e.Message);
            }
        }
    }
}